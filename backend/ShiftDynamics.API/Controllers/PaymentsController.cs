using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public PaymentsController(ShiftDynamicsDbContext db) => _db = db;

    public record CreatePaymentRequest(Guid InvoiceId, decimal Amount, PaymentMethod Method, string? TransactionReference, string? Notes);

    [HttpPost]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreatePaymentRequest request)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var customerId = User.RequireCustomerId();
        var invoice = await _db.Invoices.Include(i => i.WorkOrder).FirstOrDefaultAsync(i => i.Id == request.InvoiceId && i.WorkOrder.CustomerId == customerId)
            ?? throw new NotFoundException("Invoice not found.");

        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
            throw new ConflictException("Invoice cannot accept payments in its current status.");

        if (request.Amount <= 0 || request.Amount > invoice.BalanceDue)
            throw new ValidationException($"Payment amount must be between 0.01 and {invoice.BalanceDue}.");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            Method = request.Method,
            Status = PaymentStatus.Completed,
            TransactionReference = request.TransactionReference,
            Notes = request.Notes,
            PaymentDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        invoice.AmountPaid += request.Amount;
        invoice.BalanceDue = invoice.TotalAmount - invoice.AmountPaid;
        invoice.Status = invoice.BalanceDue <= 0 ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        invoice.UpdatedAt = DateTime.UtcNow;

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(ApiResponse<object>.Ok(payment, "Payment recorded."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] Guid? invoiceId)
    {
        var customerId = User.RequireCustomerId();
        var query = _db.Payments.AsNoTracking().Include(p => p.Invoice).ThenInclude(i => i.WorkOrder).Where(p => p.Invoice.WorkOrder.CustomerId == customerId).AsQueryable();
        if (invoiceId.HasValue) query = query.Where(p => p.InvoiceId == invoiceId);
        var items = await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }
}
