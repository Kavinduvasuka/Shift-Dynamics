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
        var invoice = await _db.Invoices.Include(i => i.WorkOrder)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && i.WorkOrder.CustomerId == customerId)
            ?? throw new NotFoundException("Invoice not found.");

        if (invoice.Status is not (
            InvoiceStatus.Issued or
            InvoiceStatus.PartiallyPaid or
            InvoiceStatus.Overdue))
        {
            throw new ConflictException(
                "Invoice cannot accept payments in its current status.");
        }

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

        return Ok(ApiResponse<object>.Ok(new
        {
            payment.Id,
            payment.InvoiceId,
            payment.Amount,
            payment.Method,
            payment.Status,
            payment.TransactionReference,
            payment.PaymentDate,
            InvoiceBalanceDue = invoice.BalanceDue,
            InvoiceStatus = invoice.Status
        }, "Payment recorded."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] Guid? invoiceId)
    {
        var role = User.GetRole();
        IQueryable<Payment> query = _db.Payments.AsNoTracking()
            .Include(p => p.Invoice).ThenInclude(i => i.WorkOrder).ThenInclude(w => w.Vehicle)
            .Include(p => p.Invoice).ThenInclude(i => i.WorkOrder).ThenInclude(w => w.Customer);

        if (role == "Customer")
        {
            var customerId = User.RequireCustomerId();
            query = query.Where(p => p.Invoice.WorkOrder.CustomerId == customerId);
        }
        else if (role is not ("Manager" or "Admin" or "ServiceAdvisor"))
        {
            throw new ForbiddenException();
        }

        if (invoiceId.HasValue) query = query.Where(p => p.InvoiceId == invoiceId);

        var items = await query.OrderByDescending(p => p.PaymentDate)
            .Select(p => new
            {
                p.Id,
                p.InvoiceId,
                InvoiceNumber = p.Invoice.InvoiceNumber,
                p.Amount,
                p.Method,
                p.Status,
                p.TransactionReference,
                p.PaymentDate,
                p.Notes,
                CustomerName = p.Invoice.WorkOrder.Customer.FirstName + " " + p.Invoice.WorkOrder.Customer.LastName,
                Vehicle = p.Invoice.WorkOrder.Vehicle.Make + " " + p.Invoice.WorkOrder.Vehicle.Model
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id:guid}/receipt")]
    public async Task<ActionResult<ApiResponse<object>>> Receipt(Guid id)
    {
        var payment = await _db.Payments.AsNoTracking()
            .Include(p => p.Invoice).ThenInclude(i => i.WorkOrder).ThenInclude(w => w.Customer)
            .Include(p => p.Invoice).ThenInclude(i => i.WorkOrder).ThenInclude(w => w.Vehicle)
            .Include(p => p.Invoice).ThenInclude(i => i.WorkOrder).ThenInclude(w => w.Service)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Payment not found.");

        var role = User.GetRole();
        if (role == "Customer")
        {
            var customerId = User.RequireCustomerId();
            if (payment.Invoice.WorkOrder.CustomerId != customerId)
                throw new ForbiddenException();
        }
        else if (role is not ("Manager" or "Admin" or "ServiceAdvisor"))
        {
            throw new ForbiddenException();
        }

        var receiptNumber = $"RCP-{payment.PaymentDate:yyyyMMdd}-{payment.Id.ToString("N")[..8].ToUpperInvariant()}";

        return Ok(ApiResponse<object>.Ok(new
        {
            ReceiptNumber = receiptNumber,
            InvoiceNumber = payment.Invoice.InvoiceNumber,
            PaymentAmount = payment.Amount,
            PaymentMethod = payment.Method.ToString(),
            TransactionReference = payment.TransactionReference,
            PaymentDate = payment.PaymentDate,
            PaymentStatus = payment.Status.ToString(),
            Customer = new
            {
                Name = payment.Invoice.WorkOrder.Customer.FirstName + " " + payment.Invoice.WorkOrder.Customer.LastName,
                Email = payment.Invoice.WorkOrder.Customer.Email,
                Phone = payment.Invoice.WorkOrder.Customer.Phone
            },
            Vehicle = new
            {
                payment.Invoice.WorkOrder.Vehicle.Make,
                payment.Invoice.WorkOrder.Vehicle.Model,
                payment.Invoice.WorkOrder.Vehicle.Year,
                payment.Invoice.WorkOrder.Vehicle.RegistrationNumber
            },
            Service = payment.Invoice.WorkOrder.Service.Name,
            InvoiceTotal = payment.Invoice.TotalAmount,
            AmountPaid = payment.Invoice.AmountPaid,
            BalanceDue = payment.Invoice.BalanceDue,
            Notes = payment.Notes
        }));
    }
}

