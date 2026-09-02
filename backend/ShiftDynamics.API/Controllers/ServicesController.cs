using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public ServicesController(ShiftDynamicsDbContext db) => _db = db;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<Service>>>> GetAll([FromQuery] bool activeOnly = true)
    {
        var query = _db.Services.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(s => s.IsActive);

        var items = await query.OrderBy(s => s.Name).ToListAsync();
        return Ok(ApiResponse<IEnumerable<Service>>.Ok(items));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<Service>>> GetById(Guid id)
    {
        var service = await _db.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException("Service not found.");
        return Ok(ApiResponse<Service>.Ok(service));
    }

    public record UpsertServiceRequest(string Name, string? Description, decimal BasePrice, int EstimatedDurationMinutes, bool IsActive = true);

    [HttpPost]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<Service>>> Create([FromBody] UpsertServiceRequest request)
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description,
            BasePrice = request.BasePrice,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Services.Add(service);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, ApiResponse<Service>.Ok(service));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<Service>>> Update(Guid id, [FromBody] UpsertServiceRequest request)
    {
        var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException("Service not found.");

        service.Name = request.Name.Trim();
        service.Description = request.Description;
        service.BasePrice = request.BasePrice;
        service.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
        service.IsActive = request.IsActive;
        service.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Service>.Ok(service));
    }
}
