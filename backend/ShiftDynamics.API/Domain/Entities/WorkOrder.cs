namespace ShiftDynamics.API.Domain.Entities;

public enum WorkOrderStatus
{
    Open,
    Assigned,
    InProgress,
    WaitingForParts,
    Completed,
    Cancelled
}

public class WorkOrder
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Guid VehicleId { get; set; }

    public Guid? AppointmentId { get; set; }

    public Guid ServiceId { get; set; }

    public Guid? AssignedStaffId { get; set; }

    public string WorkOrderNumber { get; set; } = string.Empty;

    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Open;

    public string? Description { get; set; }

    public string? TechnicianNotes { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;

    public Vehicle Vehicle { get; set; } = null!;

    public Appointment? Appointment { get; set; }

    public Service Service { get; set; } = null!;

    public Staff? AssignedStaff { get; set; }
}
