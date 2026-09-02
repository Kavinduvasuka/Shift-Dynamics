namespace ShiftDynamics.API.Domain.Entities;

public enum VendorRegistrationStatus
{
    Pending,
    Approved,
    Rejected
}

public class VendorRegistration
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Specialization { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public VendorRegistrationStatus Status { get; set; } = VendorRegistrationStatus.Pending;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? CreatedUserId { get; set; }
}
