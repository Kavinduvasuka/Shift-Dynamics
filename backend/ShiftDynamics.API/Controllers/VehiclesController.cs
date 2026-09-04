using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.DTOs.Vehicles;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;
    public VehiclesController(ShiftDynamicsDbContext db) => _db = db;
    private bool IsCustomer => User.IsInRole(SystemRole.Customer.ToString());
    private Guid CustomerId => User.RequireCustomerId();
    private static VehicleResponse Map(Vehicle v) => new() { Id=v.Id, CustomerId=v.CustomerId, RegistrationNumber=v.RegistrationNumber, Make=v.Make, Model=v.Model, Year=v.Year, VIN=v.VIN, Color=v.Color, CreatedAt=v.CreatedAt };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleResponse>>> GetVehicles()
    {
        var query = _db.Vehicles.AsNoTracking();
        if (IsCustomer) query = query.Where(v => v.CustomerId == CustomerId);
        return Ok((await query.OrderByDescending(v => v.CreatedAt).ToListAsync()).Select(Map));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleResponse>> GetVehicle(Guid id)
    {
        var query = _db.Vehicles.AsNoTracking().Where(v => v.Id == id);
        if (IsCustomer) query = query.Where(v => v.CustomerId == CustomerId);
        var vehicle = await query.FirstOrDefaultAsync();
        return vehicle is null ? NotFound() : Ok(Map(vehicle));
    }

    [HttpPost]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<VehicleResponse>> CreateVehicle(CreateVehicleRequest request)
    {
        var vehicle = new Vehicle { Id=Guid.NewGuid(), CustomerId=CustomerId, RegistrationNumber=request.RegistrationNumber.Trim(), Make=request.Make.Trim(), Model=request.Model.Trim(), Year=request.Year, VIN=string.IsNullOrWhiteSpace(request.VIN) ? null : request.VIN.Trim(), Color=string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim(), CreatedAt=DateTime.UtcNow };
        _db.Vehicles.Add(vehicle); await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetVehicle), new { id=vehicle.Id }, Map(vehicle));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<VehicleResponse>> UpdateVehicle(Guid id, UpdateVehicleRequest request)
    {
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.CustomerId == CustomerId);
        if (vehicle is null) return NotFound();
        vehicle.RegistrationNumber=request.RegistrationNumber.Trim(); vehicle.Make=request.Make.Trim(); vehicle.Model=request.Model.Trim(); vehicle.Year=request.Year; vehicle.VIN=string.IsNullOrWhiteSpace(request.VIN) ? null : request.VIN.Trim(); vehicle.Color=string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim();
        await _db.SaveChangesAsync(); return Ok(Map(vehicle));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Customer")]
    public async Task<IActionResult> DeleteVehicle(Guid id)
    {
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.CustomerId == CustomerId);
        if (vehicle is null) return NotFound();
        if (await _db.Appointments.AnyAsync(a => a.VehicleId == id) || await _db.WorkOrders.AnyAsync(w => w.VehicleId == id)) throw new ConflictException("Vehicles with service history cannot be deleted.");
        _db.Vehicles.Remove(vehicle); await _db.SaveChangesAsync(); return NoContent();
    }
}