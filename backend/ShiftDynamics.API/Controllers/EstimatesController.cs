using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Application.Interfaces;
using ShiftDynamics.API.Application.DTOs.Estimates;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/estimates")]
[Authorize(Roles = "Customer,ServiceAdvisor,Manager,Admin")]
public class EstimatesController : ControllerBase
{
    private readonly IEstimateService _estimates;

    public EstimatesController(IEstimateService estimates) => _estimates = estimates;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] Guid? workOrderId, [FromQuery] EstimateStatus? status)
    {
        var customerId = User.IsInRole(SystemRole.Customer.ToString()) ? User.RequireCustomerId() : (Guid?)null;
        return Ok(ApiResponse<object>.Ok(await _estimates.ListAsync(customerId, workOrderId, status)));
    }

    [HttpPost]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateEstimateRequest request)
    {
        var estimate = await _estimates.CreateAsync(request.WorkOrderId, request.LaborCost, request.PartsCost, request.TaxAmount, request.DiscountAmount, request.Notes);
        return Ok(ApiResponse<object>.Ok(estimate, "Estimate created."));
    }

    [HttpPost("{id:guid}/send")]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Send(Guid id)
    {
        var estimate = await _estimates.SendAsync(id) ?? throw new NotFoundException("Estimate not found.");
        return Ok(ApiResponse<object>.Ok(estimate, "Estimate sent to customer."));
    }

    [HttpPost("{id:guid}/decision")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<ApiResponse<object>>> Decision(Guid id, [FromBody] DecisionRequest request)
    {
        var customerId = User.RequireCustomerId();
        var estimate = await _estimates.DecideAsync(customerId, id, request.Approve, request.Comment) ?? throw new NotFoundException("Estimate not found.");
        return Ok(ApiResponse<object>.Ok(estimate, request.Approve ? "Approved." : "Rejected."));
    }
}