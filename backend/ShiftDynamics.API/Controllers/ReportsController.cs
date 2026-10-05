using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Interfaces;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = "Manager")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;
    public ReportsController(IReportService reports) => _reports = reports;
    [HttpGet("operational-summary")]
    public async Task<ActionResult<ApiResponse<object>>> OperationalSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to) => Ok(ApiResponse<object>.Ok(await _reports.GetOperationalSummaryAsync(from, to)));
}
