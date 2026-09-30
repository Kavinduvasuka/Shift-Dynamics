namespace ShiftDynamics.API.Domain.Entities;

public enum ModificationRequestStatus
{
    Pending,
    Approved,
    Rejected,
    InProgress,
    Completed,
    Cancelled
}

public class ModificationRequest
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public string Request { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public ModificationRequestStatus Status { get; set; } = ModificationRequestStatus.Pending;
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
}
