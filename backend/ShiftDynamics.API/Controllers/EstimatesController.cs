using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/estimates")]
[Authorize]
public class EstimatesController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public EstimatesController(ShiftDynamicsDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List(
        [FromQuery] Guid? workOrderId,
        [FromQuery] EstimateStatus? status)
    {
        var role = User.GetRole();

        var query = _db.Estimates
            .AsNoTracking()
            .Include(e => e.WorkOrder)
            .AsQueryable();

        if (role == SystemRole.Customer.ToString())
        {
            var customerId = User.RequireCustomerId();
            query = query.Where(e => e.WorkOrder.CustomerId == customerId);
        }
        else if (role is not ("ServiceAdvisor" or "Manager" or "Admin"))
        {
            throw new ForbiddenException();
        }

        if (workOrderId.HasValue)
            query = query.Where(e => e.WorkOrderId == workOrderId.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    public record CreateEstimateRequest(
        Guid WorkOrderId,
        decimal LaborCost,
        decimal PartsCost,
        decimal TaxAmount = 0,
        decimal DiscountAmount = 0,
        string? Notes = null);

    [HttpPost]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateEstimateRequest request)
    {
        if (request.LaborCost < 0 ||
            request.PartsCost < 0 ||
            request.TaxAmount < 0 ||
            request.DiscountAmount < 0)
        {
            throw new ValidationException("Estimate amounts cannot be negative.");
        }

        if (!await _db.WorkOrders.AnyAsync(w => w.Id == request.WorkOrderId))
            throw new NotFoundException("Work order not found.");

        var subtotal = request.LaborCost + request.PartsCost;
        var total = subtotal + request.TaxAmount - request.DiscountAmount;

        if (total < 0)
            throw new ValidationException(
                "Discount cannot exceed the estimate subtotal plus tax.");

        var count = await _db.Estimates.CountAsync() + 1;

        var estimate = new Estimate
        {
            Id = Guid.NewGuid(),
            WorkOrderId = request.WorkOrderId,
            EstimateNumber = $"EST-{DateTime.UtcNow:yyyyMMdd}-{count:D4}",
            LaborCost = request.LaborCost,
            PartsCost = request.PartsCost,
            Subtotal = subtotal,
            TaxAmount = request.TaxAmount,
            DiscountAmount = request.DiscountAmount,
            TotalAmount = total,
            Status = EstimateStatus.Draft,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Estimates.Add(estimate);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            estimate,
            "Estimate created."));
    }

    [HttpPost("{id:guid}/send")]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Send(Guid id)
    {
        var estimate = await _db.Estimates
            .Include(e => e.WorkOrder)
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new NotFoundException("Estimate not found.");

        if (estimate.Status != EstimateStatus.Draft)
            throw new ConflictException(
                "Only draft estimates can be sent.");

        estimate.Status = EstimateStatus.Sent;
        estimate.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            estimate,
            "Estimate sent to customer."));
    }

    public record DecisionRequest(bool Approve, string? Comment);

    [HttpPost("{id:guid}/decision")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<ApiResponse<object>>> Decision(
        Guid id,
        [FromBody] DecisionRequest request)
    {
        var customerId = User.RequireCustomerId();

        var estimate = await _db.Estimates
            .Include(e => e.WorkOrder)
            .FirstOrDefaultAsync(
                e => e.Id == id &&
                     e.WorkOrder.CustomerId == customerId)
            ?? throw new NotFoundException("Estimate not found.");

        if (estimate.Status != EstimateStatus.Sent)
            throw new ConflictException(
                "Estimate is not awaiting decision.");

        estimate.Status = request.Approve
            ? EstimateStatus.Approved
            : EstimateStatus.Rejected;

        estimate.CustomerApproved = request.Approve;
        estimate.ApprovedAt = request.Approve
            ? DateTime.UtcNow
            : null;

        estimate.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            estimate.Notes =
                (estimate.Notes + " | Customer: " + request.Comment).Trim();
        }

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            estimate,
            request.Approve ? "Approved." : "Rejected."));
    }
}
