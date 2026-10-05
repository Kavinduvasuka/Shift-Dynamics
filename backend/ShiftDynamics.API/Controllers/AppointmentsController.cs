using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Application.DTOs.Appointments;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointments;
    public AppointmentsController(IAppointmentService appointments) => _appointments = appointments;
    private Guid CustomerId => User.RequireCustomerId();

    [HttpGet]
    [Authorize(Policy="Customer")]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAppointments() => Ok(await _appointments.GetAllAsync(CustomerId));

    [HttpGet("{id:guid}")]
    [Authorize(Policy="Customer")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointment(Guid id) { var a=await _appointments.GetByIdAsync(id,CustomerId); return a is null ? NotFound() : Ok(a); }

    [HttpPost]
    [Authorize(Policy="Customer")]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointment(CreateAppointmentRequest request)
    {
        var appointment = await _appointments.CreateAsync(CustomerId, request);
        return CreatedAtAction(nameof(GetAppointment),new {id=appointment.Id},appointment);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy="Customer")]
    public async Task<ActionResult<AppointmentResponse>> UpdateAppointment(Guid id, UpdateAppointmentRequest request)
    {
        var appointment = await _appointments.UpdateAsync(id, CustomerId, request);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy="Customer")]
    public async Task<IActionResult> DeleteAppointment(Guid id) => await _appointments.CancelAsync(id, CustomerId) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy="ServiceAdvisor")]
    public async Task<ActionResult<AppointmentResponse>> ConfirmAppointment(Guid id)
    {
        var appointment = await _appointments.ConfirmAsync(id);
        return appointment is null ? NotFound() : Ok(appointment);
    }
}

