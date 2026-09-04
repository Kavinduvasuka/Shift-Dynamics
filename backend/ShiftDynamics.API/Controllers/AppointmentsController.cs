using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.DTOs.Appointments;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;
    public AppointmentsController(ShiftDynamicsDbContext db) => _db = db;
    private Guid CustomerId => User.RequireCustomerId();
    private static AppointmentResponse Map(Appointment a) => new() { Id=a.Id, CustomerId=a.CustomerId, VehicleId=a.VehicleId, AppointmentDate=a.AppointmentDate, ServiceType=a.ServiceType, Notes=a.Notes, Status=a.Status, CreatedAt=a.CreatedAt, CustomerName=$"{a.Customer.FirstName} {a.Customer.LastName}", VehicleRegistration=a.Vehicle.RegistrationNumber };
    private IQueryable<Appointment> CustomerAppointments() => _db.Appointments.Include(a=>a.Customer).Include(a=>a.Vehicle).Where(a=>a.CustomerId==CustomerId);

    [HttpGet]
    [Authorize(Policy="Customer")]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAppointments() => Ok((await CustomerAppointments().AsNoTracking().OrderBy(a=>a.AppointmentDate).ToListAsync()).Select(Map));

    [HttpGet("{id:guid}")]
    [Authorize(Policy="Customer")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointment(Guid id) { var a=await CustomerAppointments().AsNoTracking().FirstOrDefaultAsync(a=>a.Id==id); return a is null ? NotFound() : Ok(Map(a)); }

    [HttpPost]
    [Authorize(Policy="Customer")]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointment(CreateAppointmentRequest request)
    {
        if (request.AppointmentDate <= DateTime.UtcNow) throw new ValidationException("Appointment date must be in the future.");
        var vehicle=await _db.Vehicles.FirstOrDefaultAsync(v=>v.Id==request.VehicleId && v.CustomerId==CustomerId) ?? throw new NotFoundException("Vehicle not found.");
        var a=new Appointment { Id=Guid.NewGuid(), CustomerId=CustomerId, VehicleId=vehicle.Id, AppointmentDate=request.AppointmentDate, ServiceType=request.ServiceType.Trim(), Notes=string.IsNullOrWhiteSpace(request.Notes)?null:request.Notes.Trim(), Status=AppointmentStatus.Scheduled, CreatedAt=DateTime.UtcNow };
        _db.Appointments.Add(a); await _db.SaveChangesAsync(); a.Customer=(await _db.Customers.FindAsync(CustomerId))!; a.Vehicle=vehicle; return CreatedAtAction(nameof(GetAppointment),new {id=a.Id},Map(a));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy="Customer")]
    public async Task<ActionResult<AppointmentResponse>> UpdateAppointment(Guid id, UpdateAppointmentRequest request)
    {
        var a=await CustomerAppointments().FirstOrDefaultAsync(a=>a.Id==id); if(a is null) return NotFound();
        if(a.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled) throw new ConflictException("Completed or cancelled appointments cannot be changed.");
        if(request.AppointmentDate<=DateTime.UtcNow) throw new ValidationException("Appointment date must be in the future.");
        a.AppointmentDate=request.AppointmentDate; a.ServiceType=request.ServiceType.Trim(); a.Notes=string.IsNullOrWhiteSpace(request.Notes)?null:request.Notes.Trim(); await _db.SaveChangesAsync(); return Ok(Map(a));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy="Customer")]
    public async Task<IActionResult> DeleteAppointment(Guid id) { var a=await _db.Appointments.FirstOrDefaultAsync(a=>a.Id==id && a.CustomerId==CustomerId); if(a is null)return NotFound(); if(a.Status!=AppointmentStatus.Scheduled)throw new ConflictException("Only scheduled appointments may be cancelled."); a.Status=AppointmentStatus.Cancelled; await _db.SaveChangesAsync(); return NoContent(); }
}