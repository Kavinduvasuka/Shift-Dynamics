using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.DTOs.Services;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public ServicesController(ShiftDynamicsDbContext db) => _db = db;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] bool activeOnly = true, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        if (page < 1 || pageSize is < 1 or > 100) throw new ValidationException("Page must be at least 1 and page size must be between 1 and 100.");
        var query = _db.Services.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(s => s.IsActive);
        var total = await query.CountAsync();
        var items = await query.OrderBy(s => s.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(ApiResponse<object>.Ok(new PagedResult<Service> { Items = items, Page = page, PageSize = pageSize, TotalCount = total }));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<Service>>> GetById(Guid id)
    {
        var service = await _db.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException("Service not found.");
        return Ok(ApiResponse<Service>.Ok(service));
    }

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
