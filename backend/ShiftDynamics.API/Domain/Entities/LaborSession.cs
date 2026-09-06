namespace ShiftDynamics.API.Domain.Entities;

public enum LaborSessionStatus
{
    Active,
    Paused,
    Ended
}

public class LaborSession
{
    public Guid Id { get; set; }
    public Guid WorkOrderId { get; set; }
    public Guid MechanicStaffId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PausedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int PauseSeconds { get; set; }
    public int? DurationSeconds { get; set; }
    public LaborSessionStatus Status { get; set; } = LaborSessionStatus.Active;

    public WorkOrder WorkOrder { get; set; } = null!;
    public Staff Mechanic { get; set; } = null!;
}
