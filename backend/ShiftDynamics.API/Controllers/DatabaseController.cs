using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/database")]
public class DatabaseController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _dbContext;

    public DatabaseController(ShiftDynamicsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        var canConnect = await _dbContext.Database.CanConnectAsync();

        return Ok(new
        {
            database = "shift_dynamics",
            connected = canConnect,
            timestamp = DateTime.UtcNow
        });
    }
}
