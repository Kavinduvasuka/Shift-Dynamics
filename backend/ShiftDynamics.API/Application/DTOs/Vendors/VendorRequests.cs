using System.ComponentModel.DataAnnotations;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.DTOs.Vendors;

public class VendorRegisterRequest
{
    [Required, StringLength(200)] public string BusinessName { get; set; } = string.Empty;
    [Required, StringLength(150)] public string ContactPerson { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Mobile { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [StringLength(500)] public string? Address { get; set; }
    [StringLength(200)] public string? Specialization { get; set; }
    [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = string.Empty;
}

public record ReviewVendorRequest(bool Approve, string? RejectionReason);
public record UpdateVendorStatusRequest(VendorApprovalStatus Status);
