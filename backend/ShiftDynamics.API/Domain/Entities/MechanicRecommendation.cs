namespace ShiftDynamics.API.Domain.Entities;

public class MechanicRecommendation
{
    public Guid Id { get; set; }
    public Guid WorkOrderId { get; set; }
    public Guid MechanicStaffId { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public WorkOrder WorkOrder { get; set; } = null!;
    public Staff Mechanic { get; set; } = null!;
}
