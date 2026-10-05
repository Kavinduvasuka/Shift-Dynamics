using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Interfaces;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/service-history")]
[Authorize(Policy = "Customer")]
public class ServiceHistoryController : ControllerBase
{
    private readonly IServiceHistoryService _history;
    public ServiceHistoryController(IServiceHistoryService history) => _history = history;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] Guid? vehicleId) =>
        Ok(ApiResponse<object>.Ok(await _history.GetForCustomerAsync(User.RequireCustomerId(), vehicleId)));
}
