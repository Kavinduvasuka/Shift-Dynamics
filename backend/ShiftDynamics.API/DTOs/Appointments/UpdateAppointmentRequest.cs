using System.ComponentModel.DataAnnotations;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.DTOs.Appointments;

public class UpdateAppointmentRequest
{
    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Service type is required.")]
    [StringLength(200, MinimumLength = 1)]
    public string ServiceType { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public AppointmentStatus Status { get; set; }
}
