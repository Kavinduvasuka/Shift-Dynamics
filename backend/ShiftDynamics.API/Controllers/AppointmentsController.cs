using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.DTOs.Appointments;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _context;

    public AppointmentsController(ShiftDynamicsDbContext context)
    {
        _context = context;
    }

    // GET: api/appointments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAppointments()
    {
        var appointments = await _context.Appointments
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Vehicle)
            .OrderBy(x => x.AppointmentDate)
            .Select(x => new AppointmentResponse
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                VehicleId = x.VehicleId,
                AppointmentDate = x.AppointmentDate,
                ServiceType = x.ServiceType,
                Notes = x.Notes,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
                VehicleRegistration = x.Vehicle.RegistrationNumber
            })
            .ToListAsync();

        return Ok(appointments);
    }

    // GET: api/appointments/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointment(Guid id)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Vehicle)
            .Where(x => x.Id == id)
            .Select(x => new AppointmentResponse
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                VehicleId = x.VehicleId,
                AppointmentDate = x.AppointmentDate,
                ServiceType = x.ServiceType,
                Notes = x.Notes,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
                VehicleRegistration = x.Vehicle.RegistrationNumber
            })
            .FirstOrDefaultAsync();

        if (appointment is null)
        {
            return NotFound(new { message = "Appointment not found." });
        }

        return Ok(appointment);
    }

    // POST: api/appointments
    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointment(
        CreateAppointmentRequest request)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.CustomerId);

        if (customer is null)
        {
            return BadRequest(new { message = "Customer not found." });
        }

        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.VehicleId);

        if (vehicle is null)
        {
            return BadRequest(new { message = "Vehicle not found." });
        }

        if (vehicle.CustomerId != request.CustomerId)
        {
            return BadRequest(new
            {
                message = "The selected vehicle does not belong to the selected customer."
            });
        }

        if (request.AppointmentDate <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message = "Appointment date must be in the future."
            });
        }

        if (string.IsNullOrWhiteSpace(request.ServiceType))
        {
            return BadRequest(new
            {
                message = "Service type is required."
            });
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            VehicleId = request.VehicleId,
            AppointmentDate = request.AppointmentDate,
            ServiceType = request.ServiceType.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes)
                ? null
                : request.Notes.Trim(),
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var response = new AppointmentResponse
        {
            Id = appointment.Id,
            CustomerId = appointment.CustomerId,
            VehicleId = appointment.VehicleId,
            AppointmentDate = appointment.AppointmentDate,
            ServiceType = appointment.ServiceType,
            Notes = appointment.Notes,
            Status = appointment.Status,
            CreatedAt = appointment.CreatedAt,
            CustomerName = $"{customer.FirstName} {customer.LastName}",
            VehicleRegistration = vehicle.RegistrationNumber
        };

        return CreatedAtAction(
            nameof(GetAppointment),
            new { id = appointment.Id },
            response);
    }

    // PUT: api/appointments/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAppointment(
        Guid id,
        UpdateAppointmentRequest request)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(x => x.Id == id);

        if (appointment is null)
        {
            return NotFound(new { message = "Appointment not found." });
        }

        if (request.AppointmentDate <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message = "Appointment date must be in the future."
            });
        }

        if (string.IsNullOrWhiteSpace(request.ServiceType))
        {
            return BadRequest(new
            {
                message = "Service type is required."
            });
        }

        appointment.AppointmentDate = request.AppointmentDate;
        appointment.ServiceType = request.ServiceType.Trim();
        appointment.Notes = string.IsNullOrWhiteSpace(request.Notes)
            ? null
            : request.Notes.Trim();
        appointment.Status = request.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/appointments/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAppointment(Guid id)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(x => x.Id == id);

        if (appointment is null)
        {
            return NotFound(new { message = "Appointment not found." });
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
