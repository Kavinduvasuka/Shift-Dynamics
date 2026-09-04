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
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] InvoiceStatus? status)
    {
        var query = _db.Invoices.AsNoTracking().Include(i => i.WorkOrder).AsQueryable();
        if (User.IsInRole(SystemRole.Customer.ToString())) { var customerId = User.RequireCustomerId(); query = query.Where(i => i.WorkOrder.CustomerId == customerId); }
        if (status.HasValue) query = query.Where(i => i.Status == status);
        var items = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    public record CreateInvoiceRequest(Guid WorkOrderId, Guid? EstimateId, decimal LaborCost, decimal PartsCost, decimal TaxAmount = 0, decimal DiscountAmount = 0, string? Notes = null);

    [HttpPost]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateInvoiceRequest request)
    {
        if (request.LaborCost < 0 || request.PartsCost < 0 || request.TaxAmount < 0 || request.DiscountAmount < 0) throw new ValidationException("Invoice amounts cannot be negative.");
        if (!await _db.WorkOrders.AnyAsync(w => w.Id == request.WorkOrderId))
            throw new NotFoundException("Work order not found.");

        var subtotal = request.LaborCost + request.PartsCost;
        var total = subtotal + request.TaxAmount - request.DiscountAmount;
        if (total < 0) throw new ValidationException("Discount cannot exceed the invoice subtotal plus tax.");
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
        return Ok(ApiResponse<object>.Ok(invoice, "Invoice created."));
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> Approve(Guid id)
    {
        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new NotFoundException("Invoice not found.");

        invoice.Status = InvoiceStatus.Issued;
        invoice.IssuedAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(invoice, "Invoice approved and issued."));
    }
}
