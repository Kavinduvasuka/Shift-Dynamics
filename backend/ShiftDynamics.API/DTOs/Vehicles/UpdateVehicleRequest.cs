using System.ComponentModel.DataAnnotations;

namespace ShiftDynamics.API.DTOs.Vehicles;

public class UpdateVehicleRequest
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required(ErrorMessage = "Registration number is required.")]
    [StringLength(30, MinimumLength = 1)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Make is required.")]
    [StringLength(100, MinimumLength = 1)]
    public string Make { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model is required.")]
    [StringLength(100, MinimumLength = 1)]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
    public int Year { get; set; }

    [StringLength(50)]
    public string? VIN { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }
}
