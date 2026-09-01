namespace ShiftDynamics.API.Domain.Entities;

public enum EstimateStatus
{
    Draft,
    Sent,
    Approved,
    Rejected,
    Expired
}

public class Estimate
{
    public Guid Id { get; set; }

    public Guid WorkOrderId { get; set; }

    public string EstimateNumber { get; set; } = string.Empty;

    public decimal LaborCost { get; set; }

    public decimal PartsCost { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public EstimateStatus Status { get; set; } = EstimateStatus.Draft;

    public bool CustomerApproved { get; set; } = false;

    public DateTime? ApprovedAt { get; set; }

    public string? Notes { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public WorkOrder WorkOrder { get; set; } = null!;
}
