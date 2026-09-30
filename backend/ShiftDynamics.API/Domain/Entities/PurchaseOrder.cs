namespace ShiftDynamics.API.Domain.Entities;

public enum PurchaseOrderStatus
{
    PendingVendorAcceptance,
    Accepted,
    Processing,
    Shipped,
    Delivered,
    Received,
    Cancelled
}

public class PurchaseOrder
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid QuoteRequestId { get; set; }
    public Guid VendorQuoteId { get; set; }
    public Guid VendorProfileId { get; set; }
    public Guid PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.PendingVendorAcceptance;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }

    public VendorQuoteRequest QuoteRequest { get; set; } = null!;
    public VendorQuote VendorQuote { get; set; } = null!;
    public VendorProfile VendorProfile { get; set; } = null!;
    public Part Part { get; set; } = null!;
}
