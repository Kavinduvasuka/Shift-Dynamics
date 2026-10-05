using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/modification-requests")]
[Authorize]
public class ModificationRequestsController : ControllerBase
{
    private readonly IModificationRequestService _requests;
    public ModificationRequestsController(IModificationRequestService requests) => _requests = requests;
    public record CreateRequest(Guid VehicleId, [Required, StringLength(100)] string RequestType, [Required, StringLength(2000)] string Description);
    public record ReviewRequest(ModificationRequestStatus Status, decimal? ProposedCost, string? AdvisorNotes);

    [HttpPost]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<ApiResponse<object>>> Create(CreateRequest request)
    {
        var item = await _requests.CreateAsync(User.RequireCustomerId(), request.VehicleId, request.RequestType, request.Description);
        return Ok(ApiResponse<object>.Ok(item, "Modification request submitted."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] ModificationRequestStatus? status)
    {
        var customerId = User.IsInRole(SystemRole.Customer.ToString()) ? User.RequireCustomerId() : (Guid?)null;
        return Ok(ApiResponse<object>.Ok(await _requests.ListAsync(customerId, status)));
    }

    [HttpPatch("{id:guid}/review")]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Review(Guid id, ReviewRequest request)
    {
        var item = await _requests.ReviewAsync(id, request.Status, request.ProposedCost, request.AdvisorNotes);
        return item is null ? NotFound() : Ok(ApiResponse<object>.Ok(item));
    }
}

