namespace ShiftDynamics.API.Domain.Entities;

public enum VendorApprovalStatus
{
    Active = 0,
    Suspended = 1,
    Inactive = 2
}

/// <summary>
/// Long-lived vendor business profile, created when a VendorRegistration is approved.
/// Linked 1:1 to the authenticated User account (Role = Vendor).
/// </summary>
public class VendorProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? RegistrationId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? Specialization { get; set; }

    public VendorApprovalStatus ApprovalStatus { get; set; } = VendorApprovalStatus.Active;

    public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public VendorRegistration? Registration { get; set; }
}
