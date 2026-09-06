using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _dbContext;
    private readonly IHostEnvironment _env;

    public HealthController(ShiftDynamicsDbContext dbContext, IHostEnvironment env)
    {
        _dbContext = dbContext;
        _env = env;
    }

    /// <summary>
    /// Basic liveness probe – always returns 200 if the process is running.
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            service = "Shift Dynamics API",
            environment = _env.EnvironmentName,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Readiness probe – checks database connectivity.
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

        var payload = new
        {
            status = canConnect ? "ready" : "not_ready",
            database = canConnect,
            timestamp = DateTime.UtcNow
        };

        return canConnect ? Ok(payload) : StatusCode(StatusCodes.Status503ServiceUnavailable, payload);
    }
}
