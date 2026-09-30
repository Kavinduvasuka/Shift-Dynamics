namespace ShiftDynamics.API.Domain.Entities;

public enum ProcurementRequestStatus
{
    Open,
    Awarded,
    Cancelled
}

public class VendorQuoteRequest
{
    public Guid Id { get; set; }
    public Guid PartId { get; set; }
    public int Quantity { get; set; }
    public string? Specifications { get; set; }
    public ProcurementRequestStatus Status { get; set; } = ProcurementRequestStatus.Open;
    public Guid RequestedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public Part Part { get; set; } = null!;
}
