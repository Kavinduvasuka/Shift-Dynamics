namespace ShiftDynamics.API.Domain.Entities;

public enum InvoiceStatus
{
    Draft,
    Issued,
    PartiallyPaid,
    Paid,
    Overdue,
    Cancelled
}

public class Invoice
{
    public Guid Id { get; set; }

    public Guid WorkOrderId { get; set; }

    public Guid? EstimateId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public decimal LaborCost { get; set; }

    public decimal PartsCost { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal BalanceDue { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public DateTime? IssuedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public WorkOrder WorkOrder { get; set; } = null!;

    public Estimate? Estimate { get; set; }
}
