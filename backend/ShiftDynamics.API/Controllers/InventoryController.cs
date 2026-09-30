using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using System.Security.Claims;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Policy = "Storekeeper")]
public class InventoryController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public InventoryController(ShiftDynamicsDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] string? search, [FromQuery] bool lowStockOnly = false)
    {
        var query = _db.InventoryItems
            .AsNoTracking()
            .Include(i => i.Part)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(i =>
                i.Part.Name.ToLower().Contains(s) ||
                i.Part.PartNumber.ToLower().Contains(s) ||
                (i.Part.Category != null && i.Part.Category.ToLower().Contains(s)));
        }

        if (lowStockOnly)
            query = query.Where(i => i.OnHandQty <= i.ReorderLevel);

        var items = await query
            .OrderBy(i => i.Part.Name)
            .Select(i => new
            {
                i.Id,
                i.PartId,
                i.Part.PartNumber,
                i.Part.Name,
                i.Part.Category,
                i.OnHandQty,
                i.ReservedQty,
                i.ReorderLevel,
                i.UnitCost,
                i.Location,
                IsLowStock = i.OnHandQty <= i.ReorderLevel,
                i.UpdatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    public record UpsertPartRequest(
        string PartNumber, string Name, string? Description, string? Category,
        string? Compatibility, int OnHandQty, int ReorderLevel, decimal UnitCost, string? Location);

    [HttpPost("parts")]
    public async Task<ActionResult<ApiResponse<object>>> CreatePart([FromBody] UpsertPartRequest request)
    {
        if (request.OnHandQty < 0 || request.ReorderLevel < 0 || request.UnitCost < 0) throw new ValidationException("Inventory quantities and cost cannot be negative.");
        if (await _db.Parts.AnyAsync(p => p.PartNumber == request.PartNumber.Trim()))
            throw new ConflictException("Part number already exists.");

        var part = new Part
        {
            Id = Guid.NewGuid(),
            PartNumber = request.PartNumber.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description,
            Category = request.Category,
            Compatibility = request.Compatibility,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var inventory = new InventoryItem
        {
            Id = Guid.NewGuid(),
            PartId = part.Id,
            OnHandQty = request.OnHandQty,
            ReorderLevel = request.ReorderLevel,
            UnitCost = request.UnitCost,
            Location = request.Location,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Parts.Add(part);
        _db.InventoryItems.Add(inventory);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { part, inventory }, "Part created."));
    }

    [HttpGet("requisitions")]
    public async Task<ActionResult<ApiResponse<object>>> Requisitions([FromQuery] RequisitionStatus? status)
    {
        var query = _db.PartRequisitions
            .AsNoTracking()
            .Include(r => r.WorkOrder)
            .Include(r => r.RequestedBy)
            .Include(r => r.Part)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var items = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    public record ReviewRequisitionRequest(bool Approve, string? Notes);

    [HttpPost("requisitions/{id:guid}/review")]
    public async Task<ActionResult<ApiResponse<object>>> Review(Guid id, [FromBody] ReviewRequisitionRequest request)
    {
        var req = await _db.PartRequisitions.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("Requisition not found.");

        if (req.Status != RequisitionStatus.Pending)
            throw new ConflictException("Requisition is not pending.");

        req.Status = request.Approve ? RequisitionStatus.Approved : RequisitionStatus.Rejected;
        req.ReviewNotes = request.Notes;
        req.ReviewedAt = DateTime.UtcNow;
        req.ReviewedByUserId = User.RequireUserId();

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(req, request.Approve ? "Approved." : "Rejected."));
    }

    [HttpPost("requisitions/{id:guid}/release")]
    public async Task<ActionResult<ApiResponse<object>>> Release(Guid id)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var req = await _db.PartRequisitions
            .Include(r => r.Part)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("Requisition not found.");

        if (req.Status != RequisitionStatus.Approved)
            throw new ConflictException("Only approved requisitions can be released.");

        if (req.PartId is null)
            throw new ValidationException("Requisition has no linked part for stock release.");

        var inventory = await _db.InventoryItems.FirstOrDefaultAsync(i => i.PartId == req.PartId)
            ?? throw new NotFoundException("Inventory item not found.");

        if (inventory.OnHandQty < req.QtyRequested)
            throw new ConflictException($"Insufficient stock. On hand: {inventory.OnHandQty}, requested: {req.QtyRequested}.");

        inventory.OnHandQty -= req.QtyRequested;
        inventory.UpdatedAt = DateTime.UtcNow;
        req.QtyReleased = req.QtyRequested;
        req.Status = RequisitionStatus.Released;

        _db.StockMovements.Add(new StockMovement
        {
            Id = Guid.NewGuid(),
            PartId = req.PartId.Value,
            RequisitionId = req.Id,
            Type = StockMovementType.Release,
            Quantity = req.QtyRequested,
            PerformedByUserId = User.RequireUserId(),
            Reference = req.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(ApiResponse<object>.Ok(req, "Stock released."));
    }


    // ---------- Stock adjustments & movements ----------

    public record StockAdjustRequest(Guid PartId, int QuantityDelta, string? Reason);

    [HttpPost("adjust")]
    public async Task<ActionResult<ApiResponse<object>>> Adjust([FromBody] StockAdjustRequest request)
    {
        if (request.QuantityDelta == 0)
            throw new ValidationException("Quantity delta cannot be zero.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var inventory = await _db.InventoryItems.FirstOrDefaultAsync(i => i.PartId == request.PartId)
            ?? throw new NotFoundException("Inventory item not found.");

        var newQty = inventory.OnHandQty + request.QuantityDelta;
        if (newQty < 0)
            throw new ConflictException($"Adjustment would result in negative stock (current: {inventory.OnHandQty}).");

        inventory.OnHandQty = newQty;
        inventory.UpdatedAt = DateTime.UtcNow;

        _db.StockMovements.Add(new StockMovement
        {
            Id = Guid.NewGuid(),
            PartId = request.PartId,
            Type = request.QuantityDelta > 0 ? StockMovementType.Receive : StockMovementType.Adjustment,
            Quantity = Math.Abs(request.QuantityDelta),
            PerformedByUserId = User.RequireUserId(),
            Reference = request.Reason,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(ApiResponse<object>.Ok(new { inventory.PartId, inventory.OnHandQty }, "Stock adjusted."));
    }

    [HttpGet("movements")]
    public async Task<ActionResult<ApiResponse<object>>> Movements([FromQuery] Guid? partId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.StockMovements.AsNoTracking().Include(m => m.Part).AsQueryable();
        if (partId.HasValue) query = query.Where(m => m.PartId == partId.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new
            {
                m.Id,
                m.PartId,
                PartNumber = m.Part.PartNumber,
                PartName = m.Part.Name,
                m.Type,
                m.Quantity,
                m.Reference,
                m.PerformedByUserId,
                m.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { total, page, pageSize, items }));
    }

    // ---------- Procurement: quotation requests ----------

    public record CreateQuoteRequestDto(Guid PartId, int Quantity, string? Specifications);

    [HttpPost("quote-requests")]
    public async Task<ActionResult<ApiResponse<object>>> CreateQuoteRequest([FromBody] CreateQuoteRequestDto request)
    {
        if (request.Quantity <= 0) throw new ValidationException("Quantity must be positive.");
        var part = await _db.Parts.FirstOrDefaultAsync(p => p.Id == request.PartId && p.IsActive)
            ?? throw new NotFoundException("Part not found.");

        var entity = new VendorQuoteRequest
        {
            Id = Guid.NewGuid(),
            PartId = part.Id,
            Quantity = request.Quantity,
            Specifications = request.Specifications?.Trim(),
            Status = ProcurementRequestStatus.Open,
            RequestedByUserId = User.RequireUserId(),
            CreatedAt = DateTime.UtcNow
        };

        _db.VendorQuoteRequests.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            entity.Id,
            entity.PartId,
            PartNumber = part.PartNumber,
            PartName = part.Name,
            entity.Quantity,
            entity.Specifications,
            entity.Status,
            entity.CreatedAt
        }, "Quotation request created."));
    }

    [HttpGet("quote-requests")]
    public async Task<ActionResult<ApiResponse<object>>> ListQuoteRequests([FromQuery] ProcurementRequestStatus? status)
    {
        var query = _db.VendorQuoteRequests.AsNoTracking().Include(r => r.Part).AsQueryable();
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);

        var items = await query.OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.PartId,
                PartNumber = r.Part.PartNumber,
                PartName = r.Part.Name,
                r.Quantity,
                r.Specifications,
                r.Status,
                r.RequestedByUserId,
                r.CreatedAt,
                r.ClosedAt,
                QuoteCount = _db.VendorQuotes.Count(q => q.QuoteRequestId == r.Id)
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("quote-requests/{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetQuoteRequest(Guid id)
    {
        var r = await _db.VendorQuoteRequests.AsNoTracking().Include(x => x.Part)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Quote request not found.");

        var quotes = await _db.VendorQuotes.AsNoTracking()
            .Include(q => q.VendorProfile)
            .Where(q => q.QuoteRequestId == id)
            .OrderBy(q => q.UnitPrice)
            .Select(q => new
            {
                q.Id,
                q.VendorProfileId,
                VendorName = q.VendorProfile.BusinessName,
                q.UnitPrice,
                q.DeliveryDays,
                q.Notes,
                q.Status,
                q.SubmittedAt,
                Total = q.UnitPrice * r.Quantity
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            r.Id,
            r.PartId,
            PartNumber = r.Part.PartNumber,
            PartName = r.Part.Name,
            r.Quantity,
            r.Specifications,
            r.Status,
            r.CreatedAt,
            r.ClosedAt,
            Quotes = quotes
        }));
    }

    public record AwardQuoteRequest(Guid VendorQuoteId);

    [HttpPost("quote-requests/{id:guid}/award")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> AwardQuote(Guid id, [FromBody] AwardQuoteRequest request)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var quoteRequest = await _db.VendorQuoteRequests.Include(r => r.Part)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("Quote request not found.");

        if (quoteRequest.Status != ProcurementRequestStatus.Open)
            throw new ConflictException("Only open quote requests can be awarded.");

        var quote = await _db.VendorQuotes.Include(q => q.VendorProfile)
            .FirstOrDefaultAsync(q => q.Id == request.VendorQuoteId && q.QuoteRequestId == id)
            ?? throw new NotFoundException("Vendor quote not found for this request.");

        if (quote.Status != VendorQuoteStatus.Submitted)
            throw new ConflictException("Quote is not in a submittable state.");

        if (await _db.PurchaseOrders.AnyAsync(po => po.VendorQuoteId == quote.Id))
            throw new ConflictException("A purchase order already exists for this quote.");

        quote.Status = VendorQuoteStatus.Awarded;
        var otherQuotes = await _db.VendorQuotes.Where(q => q.QuoteRequestId == id && q.Id != quote.Id).ToListAsync();
        foreach (var o in otherQuotes)
            o.Status = VendorQuoteStatus.Rejected;

        quoteRequest.Status = ProcurementRequestStatus.Awarded;
        quoteRequest.ClosedAt = DateTime.UtcNow;

        var orderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var po = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            QuoteRequestId = quoteRequest.Id,
            VendorQuoteId = quote.Id,
            VendorProfileId = quote.VendorProfileId,
            PartId = quoteRequest.PartId,
            Quantity = quoteRequest.Quantity,
            UnitPrice = quote.UnitPrice,
            TotalAmount = quote.UnitPrice * quoteRequest.Quantity,
            Status = PurchaseOrderStatus.PendingVendorAcceptance,
            CreatedByUserId = User.RequireUserId(),
            CreatedAt = DateTime.UtcNow
        };

        _db.PurchaseOrders.Add(po);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            PurchaseOrder = new
            {
                po.Id,
                po.OrderNumber,
                po.Status,
                po.Quantity,
                po.UnitPrice,
                po.TotalAmount,
                VendorName = quote.VendorProfile.BusinessName,
                PartNumber = quoteRequest.Part.PartNumber
            }
        }, "Quote awarded and purchase order created."));
    }

    [HttpPost("quote-requests/{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<object>>> CancelQuoteRequest(Guid id)
    {
        var r = await _db.VendorQuoteRequests.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Quote request not found.");

        if (r.Status != ProcurementRequestStatus.Open)
            throw new ConflictException("Only open quote requests can be cancelled.");

        r.Status = ProcurementRequestStatus.Cancelled;
        r.ClosedAt = DateTime.UtcNow;

        var quotes = await _db.VendorQuotes.Where(q => q.QuoteRequestId == id && q.Status == VendorQuoteStatus.Submitted).ToListAsync();
        foreach (var q in quotes) q.Status = VendorQuoteStatus.Withdrawn;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { r.Id, r.Status }, "Quote request cancelled."));
    }

    [HttpGet("purchase-orders")]
    public async Task<ActionResult<ApiResponse<object>>> ListPurchaseOrders([FromQuery] PurchaseOrderStatus? status)
    {
        var query = _db.PurchaseOrders.AsNoTracking()
            .Include(po => po.Part)
            .Include(po => po.VendorProfile)
            .AsQueryable();
        if (status.HasValue) query = query.Where(po => po.Status == status.Value);

        var items = await query.OrderByDescending(po => po.CreatedAt)
            .Select(po => new
            {
                po.Id,
                po.OrderNumber,
                po.PartId,
                PartNumber = po.Part.PartNumber,
                PartName = po.Part.Name,
                po.VendorProfileId,
                VendorName = po.VendorProfile.BusinessName,
                po.Quantity,
                po.UnitPrice,
                po.TotalAmount,
                po.Status,
                po.CreatedAt,
                po.ReceivedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpPost("purchase-orders/{id:guid}/receive")]
    public async Task<ActionResult<ApiResponse<object>>> ReceivePurchaseOrder(Guid id, [FromBody] ReceivePoRequest? body)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var po = await _db.PurchaseOrders.Include(p => p.Part)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Purchase order not found.");

        if (po.Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled)
            throw new ConflictException("Purchase order cannot be received in its current status.");

        if (po.Status is not (PurchaseOrderStatus.Delivered or PurchaseOrderStatus.Shipped or PurchaseOrderStatus.Accepted or PurchaseOrderStatus.Processing))
            throw new ConflictException($"Cannot receive PO in status {po.Status}. Vendor must accept/process/ship first.");

        var qty = body?.Quantity ?? po.Quantity;
        if (qty <= 0 || qty > po.Quantity)
            throw new ValidationException($"Receive quantity must be between 1 and {po.Quantity}.");

        var inventory = await _db.InventoryItems.FirstOrDefaultAsync(i => i.PartId == po.PartId)
            ?? throw new NotFoundException("Inventory item not found for part.");

        inventory.OnHandQty += qty;
        if (body?.UnitCost is decimal cost && cost >= 0)
            inventory.UnitCost = cost;
        inventory.UpdatedAt = DateTime.UtcNow;

        po.Status = PurchaseOrderStatus.Received;
        po.ReceivedAt = DateTime.UtcNow;
        po.UpdatedAt = DateTime.UtcNow;

        _db.StockMovements.Add(new StockMovement
        {
            Id = Guid.NewGuid(),
            PartId = po.PartId,
            Type = StockMovementType.Receive,
            Quantity = qty,
            PerformedByUserId = User.RequireUserId(),
            Reference = po.OrderNumber,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            po.Id,
            po.OrderNumber,
            po.Status,
            ReceivedQty = qty,
            inventory.OnHandQty
        }, "Stock received into inventory."));
    }

    public record ReceivePoRequest(int? Quantity, decimal? UnitCost);
}

