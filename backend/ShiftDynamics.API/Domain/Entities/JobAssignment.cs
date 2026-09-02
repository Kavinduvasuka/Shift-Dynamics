namespace ShiftDynamics.API.Domain.Entities;

public class JobAssignment
{
    public Guid Id { get; set; }
    public Guid WorkOrderId { get; set; }
    public Guid MechanicStaffId { get; set; }
    public Guid? BayId { get; set; }
    public Guid AssignedByUserId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public WorkOrder WorkOrder { get; set; } = null!;
    public Staff Mechanic { get; set; } = null!;
    public WorkshopBay? Bay { get; set; }
}
