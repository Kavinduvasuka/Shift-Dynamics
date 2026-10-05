using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Interfaces;

namespace ShiftDynamics.API.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ShiftDynamicsDbContext _db;
    private readonly INotificationService _notifications;
    public InvoiceService(ShiftDynamicsDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<IReadOnlyList<Invoice>> ListAsync(Guid? customerId, InvoiceStatus? status)
    {
        var query = _db.Invoices.AsNoTracking().Include(i => i.WorkOrder).AsQueryable();
        if (customerId.HasValue) query = query.Where(i => i.WorkOrder.CustomerId == customerId.Value);
        if (status.HasValue) query = query.Where(i => i.Status == status.Value);
        return await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
    }

    public async Task<Invoice> CreateAsync(Guid workOrderId, Guid? estimateId, decimal laborCost, decimal partsCost, decimal taxAmount, decimal discountAmount, string? notes)
    {
        if (laborCost < 0 || partsCost < 0 || taxAmount < 0 || discountAmount < 0) throw new ValidationException("Invoice amounts cannot be negative.");
        var workOrder = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == workOrderId) ?? throw new NotFoundException("Work order not found.");
        if (estimateId.HasValue && !await _db.Estimates.AnyAsync(e => e.Id == estimateId && e.WorkOrderId == workOrderId && e.Status == EstimateStatus.Approved)) throw new ValidationException("Invoice estimate must be approved and belong to the work order.");
        var subtotal = laborCost + partsCost;
        var total = subtotal + taxAmount - discountAmount;
        if (total < 0) throw new ValidationException("Discount cannot exceed the invoice subtotal plus tax.");
        var sequence = await _db.Invoices.CountAsync() + 1;
        var invoice = new Invoice { Id = Guid.NewGuid(), WorkOrderId = workOrderId, EstimateId = estimateId, InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{sequence:D4}", LaborCost = laborCost, PartsCost = partsCost, Subtotal = subtotal, TaxAmount = taxAmount, DiscountAmount = discountAmount, TotalAmount = total, BalanceDue = total, Status = InvoiceStatus.Draft, Notes = notes?.Trim(), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice?> ApproveAsync(Guid invoiceId)
    {
        var invoice = await _db.Invoices.Include(i => i.WorkOrder).FirstOrDefaultAsync(i => i.Id == invoiceId);
        if (invoice is null) return null;
        if (invoice.Status != InvoiceStatus.Draft) throw new ConflictException("Only draft invoices can be issued.");
        invoice.Status = InvoiceStatus.Issued;
        invoice.IssuedAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        var customerUserId = await _db.Users.Where(u => u.CustomerId == invoice.WorkOrder.CustomerId).Select(u => (Guid?)u.Id).FirstOrDefaultAsync();
        if (customerUserId.HasValue) await _notifications.CreateAsync(customerUserId.Value, "invoice", "Invoice issued", $"Invoice {invoice.InvoiceNumber} is available for payment.", "Invoice", invoice.Id);
        return invoice;
    }
}
