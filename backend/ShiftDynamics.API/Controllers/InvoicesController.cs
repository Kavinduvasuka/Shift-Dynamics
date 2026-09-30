using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public InvoicesController(ShiftDynamicsDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List(
        [FromQuery] InvoiceStatus? status)
    {
        var role = User.GetRole();

        var query = _db.Invoices
            .AsNoTracking()
            .Include(i => i.WorkOrder)
            .AsQueryable();

        if (role == SystemRole.Customer.ToString())
        {
            var customerId = User.RequireCustomerId();
            query = query.Where(
                i => i.WorkOrder.CustomerId == customerId);
        }
        else if (role is not ("ServiceAdvisor" or "Manager" or "Admin"))
        {
            throw new ForbiddenException();
        }

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    public record CreateInvoiceRequest(
        Guid WorkOrderId,
        Guid? EstimateId,
        decimal LaborCost,
        decimal PartsCost,
        decimal TaxAmount = 0,
        decimal DiscountAmount = 0,
        string? Notes = null);

    [HttpPost]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateInvoiceRequest request)
    {
        if (request.LaborCost < 0 ||
            request.PartsCost < 0 ||
            request.TaxAmount < 0 ||
            request.DiscountAmount < 0)
        {
            throw new ValidationException(
                "Invoice amounts cannot be negative.");
        }

        var workOrderExists = await _db.WorkOrders
            .AnyAsync(w => w.Id == request.WorkOrderId);

        if (!workOrderExists)
            throw new NotFoundException("Work order not found.");

        if (request.EstimateId.HasValue)
        {
            var estimateMatches = await _db.Estimates.AnyAsync(
                e => e.Id == request.EstimateId.Value &&
                     e.WorkOrderId == request.WorkOrderId);

            if (!estimateMatches)
                throw new ValidationException(
                    "Estimate does not belong to the specified work order.");
        }

        var subtotal = request.LaborCost + request.PartsCost;
        var total = subtotal + request.TaxAmount - request.DiscountAmount;

        if (total < 0)
            throw new ValidationException(
                "Discount cannot exceed the invoice subtotal plus tax.");

        var count = await _db.Invoices.CountAsync() + 1;

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            WorkOrderId = request.WorkOrderId,
            EstimateId = request.EstimateId,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{count:D4}",
            LaborCost = request.LaborCost,
            PartsCost = request.PartsCost,
            Subtotal = subtotal,
            TaxAmount = request.TaxAmount,
            DiscountAmount = request.DiscountAmount,
            TotalAmount = total,
            AmountPaid = 0,
            BalanceDue = total,
            Status = InvoiceStatus.Draft,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            invoice,
            "Invoice created."));
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> Approve(Guid id)
    {
        var invoice = await _db.Invoices
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new NotFoundException("Invoice not found.");

        if (invoice.Status != InvoiceStatus.Draft)
            throw new ConflictException(
                "Only draft invoices can be approved.");

        invoice.Status = InvoiceStatus.Issued;
        invoice.IssuedAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            invoice,
            "Invoice approved and issued."));
    }
}
