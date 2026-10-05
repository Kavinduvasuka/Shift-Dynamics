using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly ShiftDynamicsDbContext _db;
    private readonly INotificationService _notifications;
    public PaymentService(ShiftDynamicsDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Payment> CreateAsync(Guid customerId, Guid invoiceId, decimal amount, PaymentMethod method, string? transactionReference, string? notes)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var invoice = await _db.Invoices.Include(i => i.WorkOrder).FirstOrDefaultAsync(i => i.Id == invoiceId && i.WorkOrder.CustomerId == customerId)
            ?? throw new NotFoundException("Invoice not found.");
        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled) throw new ConflictException("Invoice cannot accept payments in its current status.");
        if (amount <= 0 || amount > invoice.BalanceDue) throw new ValidationException($"Payment amount must be between 0.01 and {invoice.BalanceDue}.");
        if (!string.IsNullOrWhiteSpace(transactionReference) && await _db.Payments.AnyAsync(p => p.TransactionReference == transactionReference)) throw new ConflictException("A payment with this transaction reference already exists.");

        var payment = new Payment { Id = Guid.NewGuid(), InvoiceId = invoiceId, Amount = amount, Method = method, Status = PaymentStatus.Completed, TransactionReference = transactionReference?.Trim(), Notes = notes?.Trim(), PaymentDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
        invoice.AmountPaid += amount;
        invoice.BalanceDue = invoice.TotalAmount - invoice.AmountPaid;
        invoice.Status = invoice.BalanceDue <= 0 ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        invoice.UpdatedAt = DateTime.UtcNow;
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        var userId = await _db.Users.Where(u => u.CustomerId == customerId).Select(u => (Guid?)u.Id).FirstOrDefaultAsync();
        if (userId.HasValue)
            await _notifications.CreateAsync(userId.Value, "payment", "Payment recorded", $"A payment of {amount:0.00} was recorded for invoice {invoice.InvoiceNumber}.", "Invoice", invoice.Id);
        return payment;
    }

    public async Task<IReadOnlyList<Payment>> ListForCustomerAsync(Guid customerId, Guid? invoiceId)
    {
        var query = _db.Payments.AsNoTracking().Include(p => p.Invoice).ThenInclude(i => i.WorkOrder).Where(p => p.Invoice.WorkOrder.CustomerId == customerId);
        if (invoiceId.HasValue) query = query.Where(p => p.InvoiceId == invoiceId.Value);
        return await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
    }
}

