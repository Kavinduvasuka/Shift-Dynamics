namespace ShiftDynamics.API.Domain.Entities;

public enum VendorQuoteStatus
{
    Submitted,
    Awarded,
    Rejected,
    Withdrawn
}

public class VendorQuote
{
    public Guid Id { get; set; }
    public Guid QuoteRequestId { get; set; }
    public Guid VendorProfileId { get; set; }
    public decimal UnitPrice { get; set; }
    public int DeliveryDays { get; set; }
    public string? Notes { get; set; }
    public VendorQuoteStatus Status { get; set; } = VendorQuoteStatus.Submitted;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public VendorQuoteRequest QuoteRequest { get; set; } = null!;
    public VendorProfile VendorProfile { get; set; } = null!;
}
