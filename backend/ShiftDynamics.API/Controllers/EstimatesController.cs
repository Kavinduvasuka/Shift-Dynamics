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
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] Guid? workOrderId, [FromQuery] EstimateStatus? status)
    {
        var query = _db.Estimates.AsNoTracking().AsQueryable();
        if (workOrderId.HasValue) query = query.Where(e => e.WorkOrderId == workOrderId);
        if (status.HasValue) query = query.Where(e => e.Status == status);

        var items = await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    public record CreateEstimateRequest(
        Guid WorkOrderId, decimal LaborCost, decimal PartsCost,
        decimal TaxAmount = 0, decimal DiscountAmount = 0, string? Notes = null);

    [HttpPost]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateEstimateRequest request)
    {
        if (!await _db.WorkOrders.AnyAsync(w => w.Id == request.WorkOrderId))
            throw new NotFoundException("Work order not found.");

        var subtotal = request.LaborCost + request.PartsCost;
        var total = subtotal + request.TaxAmount - request.DiscountAmount;
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
        return Ok(ApiResponse<object>.Ok(estimate, "Estimate created."));
    }

    [HttpPost("{id:guid}/send")]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Send(Guid id)
    {
        var estimate = await _db.Estimates.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new NotFoundException("Estimate not found.");

        estimate.Status = EstimateStatus.Sent;
        estimate.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(estimate, "Estimate sent to customer."));
    }

    public record DecisionRequest(bool Approve, string? Comment);

    [HttpPost("{id:guid}/decision")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<ApiResponse<object>>> Decision(Guid id, [FromBody] DecisionRequest request)
    {
        var estimate = await _db.Estimates.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new NotFoundException("Estimate not found.");

        if (estimate.Status != EstimateStatus.Sent)
            throw new ConflictException("Estimate is not awaiting decision.");

        estimate.Status = request.Approve ? EstimateStatus.Approved : EstimateStatus.Rejected;
        estimate.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Comment))
            estimate.Notes = (estimate.Notes + " | Customer: " + request.Comment).Trim();

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(estimate, request.Approve ? "Approved." : "Rejected."));
    }
}
