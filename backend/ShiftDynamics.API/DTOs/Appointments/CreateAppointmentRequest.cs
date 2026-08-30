using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.DTOs.Appointments;

public class CreateAppointmentRequest
{
    public Guid CustomerId { get; set; }

    public Guid VehicleId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string ServiceType { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
}
