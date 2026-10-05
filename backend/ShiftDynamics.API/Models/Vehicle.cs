namespace ShiftDynamics.API.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public string? VIN { get; set; }

    public string? Color { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;
}
