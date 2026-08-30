namespace ShiftDynamics.API.DTOs.Vehicles;

public class CreateVehicleRequest
{
    public Guid CustomerId { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public string? VIN { get; set; }

    public string? Color { get; set; }
}
