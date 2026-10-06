using System.ComponentModel.DataAnnotations;

namespace ShiftDynamics.API.Application.DTOs.Emergency;

public class CreateEmergencyRequest
{
    public Guid? VehicleId { get; set; }
    [Required] public string Location { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    [Required] public string ProblemDescription { get; set; } = string.Empty;
    [Required] public string ContactName { get; set; } = string.Empty;
    [Required] public string ContactPhone { get; set; } = string.Empty;
}
