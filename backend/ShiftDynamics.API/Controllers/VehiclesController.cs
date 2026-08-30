using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.DTOs.Vehicles;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _dbContext;

    public VehiclesController(ShiftDynamicsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleResponse>>> GetVehicles()
    {
        var vehicles = await _dbContext.Vehicles
            .AsNoTracking()
            .Select(v => new VehicleResponse
            {
                Id = v.Id,
                CustomerId = v.CustomerId,
                RegistrationNumber = v.RegistrationNumber,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                VIN = v.VIN,
                Color = v.Color,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();

        return Ok(vehicles);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleResponse>> GetVehicle(Guid id)
    {
        var vehicle = await _dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new VehicleResponse
            {
                Id = v.Id,
                CustomerId = v.CustomerId,
                RegistrationNumber = v.RegistrationNumber,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                VIN = v.VIN,
                Color = v.Color,
                CreatedAt = v.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (vehicle is null)
            return NotFound();

        return Ok(vehicle);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleResponse>> CreateVehicle(
        CreateVehicleRequest request)
    {
        var customerExists = await _dbContext.Customers
            .AnyAsync(c => c.Id == request.CustomerId);

        if (!customerExists)
            return BadRequest(new
            {
                message = "The specified customer does not exist."
            });

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            RegistrationNumber = request.RegistrationNumber.Trim(),
            Make = request.Make.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            VIN = request.VIN?.Trim(),
            Color = request.Color?.Trim()
        };

        _dbContext.Vehicles.Add(vehicle);
        await _dbContext.SaveChangesAsync();

        var response = new VehicleResponse
        {
            Id = vehicle.Id,
            CustomerId = vehicle.CustomerId,
            RegistrationNumber = vehicle.RegistrationNumber,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            VIN = vehicle.VIN,
            Color = vehicle.Color,
            CreatedAt = vehicle.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetVehicle),
            new { id = vehicle.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateVehicle(
        Guid id,
        UpdateVehicleRequest request)
    {
        var vehicle = await _dbContext.Vehicles.FindAsync(id);

        if (vehicle is null)
            return NotFound();

        var customerExists = await _dbContext.Customers
            .AnyAsync(c => c.Id == request.CustomerId);

        if (!customerExists)
            return BadRequest(new
            {
                message = "The specified customer does not exist."
            });

        vehicle.CustomerId = request.CustomerId;
        vehicle.RegistrationNumber = request.RegistrationNumber.Trim();
        vehicle.Make = request.Make.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.Year = request.Year;
        vehicle.VIN = request.VIN?.Trim();
        vehicle.Color = request.Color?.Trim();

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVehicle(Guid id)
    {
        var vehicle = await _dbContext.Vehicles.FindAsync(id);

        if (vehicle is null)
            return NotFound();

        _dbContext.Vehicles.Remove(vehicle);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
