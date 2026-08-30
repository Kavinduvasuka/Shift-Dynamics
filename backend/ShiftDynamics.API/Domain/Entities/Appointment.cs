namespace ShiftDynamics.API.Domain.Entities;

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    Completed,
    Cancelled,
    NoShow
}

public class Appointment
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Guid VehicleId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string ServiceType { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;

    public Vehicle Vehicle { get; set; } = null!;
}
