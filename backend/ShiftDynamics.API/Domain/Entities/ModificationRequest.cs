namespace ShiftDynamics.API.Domain.Entities;

public enum ModificationRequestStatus { Submitted, UnderReview, Quoted, Approved, Rejected, Cancelled }

public class ModificationRequest
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ModificationRequestStatus Status { get; set; } = ModificationRequestStatus.Submitted;
    public decimal? ProposedCost { get; set; }
    public string? AdvisorNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Customer Customer { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
}
