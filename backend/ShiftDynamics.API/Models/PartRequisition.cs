namespace ShiftDynamics.API.Domain.Entities;

public enum RequisitionStatus
{
    Pending,
    Approved,
    Rejected,
    Released,
    Cancelled
}

public enum RequisitionUrgency
{
    Low,
    Normal,
    High,
    Critical
}

public class PartRequisition
{
    public Guid Id { get; set; }
    public Guid WorkOrderId { get; set; }
    public Guid RequestedByStaffId { get; set; }
    public Guid? PartId { get; set; }
    public string PartSpec { get; set; } = string.Empty;
    public int QtyRequested { get; set; }
    public int QtyReleased { get; set; }
    public RequisitionUrgency Urgency { get; set; } = RequisitionUrgency.Normal;
    public string? Reason { get; set; }
    public RequisitionStatus Status { get; set; } = RequisitionStatus.Pending;
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public WorkOrder WorkOrder { get; set; } = null!;
    public Staff RequestedBy { get; set; } = null!;
    public Part? Part { get; set; }
}
