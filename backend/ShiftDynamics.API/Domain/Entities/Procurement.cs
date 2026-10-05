namespace ShiftDynamics.API.Domain.Entities;

public enum QuoteRequestStatus { Open, Closed, Awarded, Cancelled }
public enum VendorQuoteStatus { Submitted, Withdrawn, Rejected, Accepted }
public enum PurchaseOrderStatus { Draft, Approved, Sent, PartiallyReceived, Received, Cancelled }

public class QuoteRequest
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public Guid? PartId { get; set; }
    public Guid? PartRequisitionId { get; set; }
    public string PartDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime RequiredBy { get; set; }
    public QuoteRequestStatus Status { get; set; } = QuoteRequestStatus.Open;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Part? Part { get; set; }
    public PartRequisition? PartRequisition { get; set; }
    public ICollection<VendorQuote> Quotes { get; set; } = new List<VendorQuote>();
}

public class VendorQuote
{
    public Guid Id { get; set; }
    public Guid QuoteRequestId { get; set; }
    public Guid VendorProfileId { get; set; }
    public decimal UnitPrice { get; set; }
    public int AvailableQuantity { get; set; }
    public int DeliveryDays { get; set; }
    public string? Notes { get; set; }
    public VendorQuoteStatus Status { get; set; } = VendorQuoteStatus.Submitted;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public QuoteRequest QuoteRequest { get; set; } = null!;
    public VendorProfile VendorProfile { get; set; } = null!;
}

public class PurchaseOrder
{
    public Guid Id { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public Guid VendorQuoteId { get; set; }
    public Guid QuoteRequestId { get; set; }
    public Guid ApprovedByUserId { get; set; }
    public int Quantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Approved;
    public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public VendorQuote VendorQuote { get; set; } = null!;
    public QuoteRequest QuoteRequest { get; set; } = null!;
}
