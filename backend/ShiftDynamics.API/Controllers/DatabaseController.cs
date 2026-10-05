using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Interfaces;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/database")]
public class DatabaseController : ControllerBase
{
    private readonly IDatabaseService _database;

    public DatabaseController(IDatabaseService database)
    {
        _database = database;
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health(CancellationToken cancellationToken)
    {
        return Ok(await _database.GetHealthAsync(cancellationToken));
    }
}
