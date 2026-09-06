using System.ComponentModel.DataAnnotations;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.DTOs.Appointments;

public class CreateAppointmentRequest
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public Guid VehicleId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Service type is required.")]
    [StringLength(200, MinimumLength = 1)]
    public string ServiceType { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
}
