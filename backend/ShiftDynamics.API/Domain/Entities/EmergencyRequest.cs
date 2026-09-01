namespace ShiftDynamics.API.Domain.Entities;

public enum EmergencyRequestStatus
{
    Pending,
    Accepted,
    InProgress,
    Completed,
    Cancelled
}

public class EmergencyRequest
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Guid? VehicleId { get; set; }

    public string Location { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string ProblemDescription { get; set; } = string.Empty;

    public EmergencyRequestStatus Status { get; set; } = EmergencyRequestStatus.Pending;

    public Guid? AssignedStaffId { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? AcceptedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;

    public Vehicle? Vehicle { get; set; }

    public Staff? AssignedStaff { get; set; }
}
