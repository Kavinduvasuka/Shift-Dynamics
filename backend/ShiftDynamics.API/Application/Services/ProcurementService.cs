using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class ProcurementService : IProcurementService
{
    private readonly ShiftDynamicsDbContext _db;
    public ProcurementService(ShiftDynamicsDbContext db) => _db = db;

    public async Task<QuoteRequest> CreateRequestAsync(Guid createdByUserId, Guid? partId, Guid? requisitionId, string partDescription, int quantity, DateTime requiredBy)
    {
        if (quantity <= 0) throw new ValidationException("Quantity must be greater than zero.");
        if (partId.HasValue && !await _db.Parts.AnyAsync(p => p.Id == partId)) throw new NotFoundException("Part not found.");
        if (requisitionId.HasValue && !await _db.PartRequisitions.AnyAsync(r => r.Id == requisitionId)) throw new NotFoundException("Part requisition not found.");
        var sequence = await _db.QuoteRequests.CountAsync() + 1;
        var request = new QuoteRequest { Id = Guid.NewGuid(), RequestNumber = $"RFQ-{DateTime.UtcNow:yyyyMMdd}-{sequence:D4}", PartId = partId, PartRequisitionId = requisitionId, PartDescription = partDescription.Trim(), Quantity = quantity, RequiredBy = requiredBy, CreatedByUserId = createdByUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _db.QuoteRequests.Add(request);
        await _db.SaveChangesAsync();
        return request;
    }

    public async Task<IReadOnlyList<QuoteRequest>> ListRequestsAsync(QuoteRequestStatus? status, Guid? vendorProfileId = null)
    {
        var query = _db.QuoteRequests.AsNoTracking().Include(r => r.Part).Include(r => r.Quotes).ThenInclude(q => q.VendorProfile).AsQueryable();
        if (status.HasValue) query = query.Where(r => r.Status == status);
        if (vendorProfileId.HasValue) query = query.Where(r => r.Quotes.Any(q => q.VendorProfileId == vendorProfileId.Value) || r.Status == QuoteRequestStatus.Open);
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<VendorQuote> SubmitQuoteAsync(Guid vendorUserId, Guid quoteRequestId, decimal unitPrice, int availableQuantity, int deliveryDays, string? notes)
    {
        if (unitPrice < 0 || availableQuantity <= 0 || deliveryDays < 0) throw new ValidationException("Quote price, quantity, and delivery time are invalid.");
        var vendor = await _db.VendorProfiles.FirstOrDefaultAsync(v => v.UserId == vendorUserId && v.ApprovalStatus == VendorApprovalStatus.Active) ?? throw new ForbiddenException("An active vendor profile is required.");
        var request = await _db.QuoteRequests.FirstOrDefaultAsync(r => r.Id == quoteRequestId) ?? throw new NotFoundException("Quote request not found.");
        if (request.Status != QuoteRequestStatus.Open) throw new ConflictException("This quote request is closed.");
        if (await _db.VendorQuotes.AnyAsync(q => q.QuoteRequestId == quoteRequestId && q.VendorProfileId == vendor.Id)) throw new ConflictException("A quote has already been submitted for this request.");
        var quote = new VendorQuote { Id = Guid.NewGuid(), QuoteRequestId = quoteRequestId, VendorProfileId = vendor.Id, UnitPrice = unitPrice, AvailableQuantity = availableQuantity, DeliveryDays = deliveryDays, Notes = notes?.Trim(), Status = VendorQuoteStatus.Submitted, SubmittedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _db.VendorQuotes.Add(quote);
        await _db.SaveChangesAsync();
        return quote;
    }

    public async Task<PurchaseOrder> AwardAsync(Guid managerUserId, Guid vendorQuoteId)
    {
        // Integration retry wrapper: AwardAsync
        return await _db.Database.CreateExecutionStrategy().ExecuteAsync<PurchaseOrder>(async () =>
        {
        _db.ChangeTracker.Clear();
        await using var tx = await _db.Database.BeginTransactionAsync();
        var quote = await _db.VendorQuotes.Include(q => q.QuoteRequest).FirstOrDefaultAsync(q => q.Id == vendorQuoteId) ?? throw new NotFoundException("Vendor quote not found.");
        if (quote.Status != VendorQuoteStatus.Submitted || quote.QuoteRequest.Status != QuoteRequestStatus.Open) throw new ConflictException("This quote cannot be awarded.");
        if (quote.AvailableQuantity < quote.QuoteRequest.Quantity) throw new ValidationException("Vendor cannot supply the requested quantity.");
        var sequence = await _db.PurchaseOrders.CountAsync() + 1;
        var order = new PurchaseOrder { Id = Guid.NewGuid(), PurchaseOrderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{sequence:D4}", VendorQuoteId = quote.Id, QuoteRequestId = quote.QuoteRequestId, ApprovedByUserId = managerUserId, Quantity = quote.QuoteRequest.Quantity, UnitPrice = quote.UnitPrice, Status = PurchaseOrderStatus.Approved, ApprovedAt = DateTime.UtcNow, ExpectedDeliveryAt = DateTime.UtcNow.AddDays(quote.DeliveryDays) };
        quote.Status = VendorQuoteStatus.Accepted;
        quote.QuoteRequest.Status = QuoteRequestStatus.Awarded;
        quote.QuoteRequest.UpdatedAt = DateTime.UtcNow;
        _db.PurchaseOrders.Add(order);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return order;
    
        });
    }

    public async Task<PurchaseOrder?> UpdateDeliveryAsync(Guid vendorUserId, Guid purchaseOrderId, DateTime? expectedDeliveryAt)
    {
        var order = await _db.PurchaseOrders.Include(o => o.VendorQuote).ThenInclude(q => q.VendorProfile).FirstOrDefaultAsync(o => o.Id == purchaseOrderId && o.VendorQuote.VendorProfile.UserId == vendorUserId);
        if (order is null) return null;
        if (order.Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled) throw new ConflictException("This purchase order is closed.");
        order.ExpectedDeliveryAt = expectedDeliveryAt;
        order.Status = PurchaseOrderStatus.Sent;
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<PurchaseOrder?> ReceiveAsync(Guid storekeeperUserId, Guid purchaseOrderId, int quantityReceived)
    {
        // Integration retry wrapper: ReceiveAsync
        return await _db.Database.CreateExecutionStrategy().ExecuteAsync<PurchaseOrder?>(async () =>
        {
        if (quantityReceived <= 0) throw new ValidationException("Received quantity must be greater than zero.");
        _db.ChangeTracker.Clear();
        await using var tx = await _db.Database.BeginTransactionAsync();
        var order = await _db.PurchaseOrders.Include(o => o.QuoteRequest).FirstOrDefaultAsync(o => o.Id == purchaseOrderId) ?? throw new NotFoundException("Purchase order not found.");
        if (order.Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled) throw new ConflictException("This purchase order is closed.");
        if (quantityReceived > order.Quantity - order.ReceivedQuantity) throw new ValidationException("Received quantity exceeds the outstanding purchase order quantity.");
        if (!order.QuoteRequest.PartId.HasValue) throw new ValidationException("The quote request must be linked to a catalog part before stock can be received.");
        var inventory = await _db.InventoryItems.FirstOrDefaultAsync(i => i.PartId == order.QuoteRequest.PartId) ?? throw new NotFoundException("Inventory item not found.");
        inventory.OnHandQty += quantityReceived;
        inventory.UnitCost = order.UnitPrice;
        inventory.UpdatedAt = DateTime.UtcNow;
        order.ReceivedQuantity += quantityReceived;
        order.Status = order.ReceivedQuantity == order.Quantity ? PurchaseOrderStatus.Received : PurchaseOrderStatus.PartiallyReceived;
        if (order.Status == PurchaseOrderStatus.Received) order.ReceivedAt = DateTime.UtcNow;
        _db.StockMovements.Add(new StockMovement { Id = Guid.NewGuid(), PartId = order.QuoteRequest.PartId.Value, Type = StockMovementType.Receive, Quantity = quantityReceived, PerformedByUserId = storekeeperUserId, Reference = order.PurchaseOrderNumber, CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return order;
    
        });
    }
}