namespace ShiftDynamics.API.Domain.Entities;

public class DiagnosticFinding
{
    public Guid Id { get; set; }
    public Guid WorkOrderId { get; set; }
    public Guid MechanicStaffId { get; set; }
    public string Finding { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public WorkOrder WorkOrder { get; set; } = null!;
    public Staff Mechanic { get; set; } = null!;
}
