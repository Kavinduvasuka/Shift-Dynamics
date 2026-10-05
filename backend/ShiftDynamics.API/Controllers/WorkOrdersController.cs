using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using System.Security.Claims;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/work-orders")]
[Authorize]
public class WorkOrdersController : ControllerBase
{
    private readonly IWorkOrderService _workOrders;

    public WorkOrdersController(IWorkOrderService workOrders) => _workOrders = workOrders;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetAll(
        [FromQuery] WorkOrderStatus? status)
    {
        var customerId = User.IsInRole(SystemRole.Customer.ToString()) ? User.RequireCustomerId() : (Guid?)null;
        var items = (await _workOrders.ListAsync(customerId, status))
            .Select(w => new
            {
                w.Id,
                w.WorkOrderNumber,
                w.Status,
                w.CustomerId,
                CustomerName = w.Customer.FirstName + " " + w.Customer.LastName,
                w.VehicleId,
                VehicleReg = w.Vehicle.RegistrationNumber,
                w.ServiceId,
                ServiceName = w.Service.Name,
                w.AssignedStaffId,
                w.Description,
                w.StartedAt,
                w.CompletedAt,
                w.CreatedAt
            }).ToList();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id)
    {
        var customerId = User.IsInRole(SystemRole.Customer.ToString()) ? User.RequireCustomerId() : (Guid?)null;
        var w = await _workOrders.GetByIdAsync(id, customerId)
            ?? throw new NotFoundException("Work order not found.");

        return Ok(ApiResponse<object>.Ok(w));
    }

    public record CreateWorkOrderRequest(
        Guid CustomerId,
        Guid VehicleId,
        Guid ServiceId,
        Guid? AppointmentId,
        string? Description);

    [HttpPost]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateWorkOrderRequest request)
    {
        var workOrder = await _workOrders.CreateAsync(request.CustomerId, request.VehicleId, request.ServiceId, request.AppointmentId, request.Description);

        return CreatedAtAction(nameof(GetById), new { id = workOrder.Id },
            ApiResponse<object>.Ok(workOrder, "Work order created."));
    }

    public record UpdateStatusRequest(WorkOrderStatus Status, string? Notes);

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var wo = await _workOrders.UpdateStatusAsync(id, request.Status, request.Notes)
            ?? throw new NotFoundException("Work order not found.");
        return Ok(ApiResponse<object>.Ok(wo, "Status updated."));
    }
}

