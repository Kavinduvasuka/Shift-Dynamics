using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _health;
    private readonly IHostEnvironment _env;

    public HealthController(IHealthService health, IHostEnvironment env)
    {
        _health = health;
        _env = env;
    }

    /// <summary>
    /// Basic liveness probe â€“ always returns 200 if the process is running.
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_health.GetLiveness(_env.EnvironmentName));
    }

    /// <summary>
    /// Readiness probe â€“ checks database connectivity.
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        var payload = await _health.GetReadinessAsync(cancellationToken);
        return (string)payload.GetType().GetProperty("status")!.GetValue(payload)! == "ready" ? Ok(payload) : StatusCode(StatusCodes.Status503ServiceUnavailable, payload);
    }
}

