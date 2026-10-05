using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using System.Security.Claims;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/emergency")]
public class EmergencyController : ControllerBase
{
    private readonly IEmergencyService _emergency;

    public EmergencyController(IEmergencyService emergency) => _emergency = emergency;

    [HttpGet("services")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> NearbyServices(
        [FromQuery] decimal? lat,
        [FromQuery] decimal? lng,
        [FromQuery] string? category,
        [FromQuery] double radiusKm = 25)
    {
        return Ok(ApiResponse<object>.Ok(await _emergency.FindProvidersAsync(lat, lng, category, radiusKm)));
    }

    public class CreateEmergencyRequest
    {
        public Guid? CustomerId { get; set; }
        public Guid? VehicleId { get; set; }
        [Required] public string Location { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        [Required] public string ProblemDescription { get; set; } = string.Empty;
        [Required] public string ContactName { get; set; } = string.Empty;
        [Required] public string ContactPhone { get; set; } = string.Empty;
    }

    [HttpPost("requests")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<ApiResponse<object>>> CreateRequest([FromBody] CreateEmergencyRequest request)
    {
        var customerId = User.RequireCustomerId();
        var entity = await _emergency.CreateRequestAsync(customerId, request.VehicleId, request.Location, request.Latitude, request.Longitude, request.ProblemDescription);

        return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.Status }, "Emergency request submitted."));
    }

    [HttpGet("requests")]
    [Authorize(Policy = "Staff")]
    public async Task<ActionResult<ApiResponse<object>>> ListRequests([FromQuery] EmergencyRequestStatus? status)
    {
        return Ok(ApiResponse<object>.Ok(await _emergency.ListRequestsAsync(status)));
    }

}

