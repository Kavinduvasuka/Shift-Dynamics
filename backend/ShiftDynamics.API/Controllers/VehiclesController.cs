using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.DTOs.Vehicles;
using ShiftDynamics.API.Interfaces;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicles;
    public VehiclesController(IVehicleService vehicles) => _vehicles = vehicles;
    private bool IsCustomer => User.IsInRole(SystemRole.Customer.ToString());
    private Guid CustomerId => User.RequireCustomerId();
    private static VehicleResponse Map(Vehicle v) => new() { Id=v.Id, CustomerId=v.CustomerId, RegistrationNumber=v.RegistrationNumber, Make=v.Make, Model=v.Model, Year=v.Year, VIN=v.VIN, Color=v.Color, CreatedAt=v.CreatedAt };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleResponse>>> GetVehicles()
    {
        return Ok(await _vehicles.GetAllAsync(IsCustomer ? CustomerId : null));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleResponse>> GetVehicle(Guid id)
    {
        var vehicle = await _vehicles.GetByIdAsync(id, IsCustomer ? CustomerId : null);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpPost]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<VehicleResponse>> CreateVehicle(CreateVehicleRequest request)
    {
        var vehicle = await _vehicles.CreateAsync(CustomerId, request);
        return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<VehicleResponse>> UpdateVehicle(Guid id, UpdateVehicleRequest request)
    {
        var vehicle = await _vehicles.UpdateAsync(id, CustomerId, request);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Customer")]
    public async Task<IActionResult> DeleteVehicle(Guid id)
    {
        return await _vehicles.DeleteAsync(id, CustomerId) ? NoContent() : NotFound();
    }
}
