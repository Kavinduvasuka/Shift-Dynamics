using System.ComponentModel.DataAnnotations;

namespace ShiftDynamics.API.Application.DTOs.Contact;

public class CreateInquiryRequest
{
    [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [StringLength(30)] public string? Phone { get; set; }
    [StringLength(50)] public string? Type { get; set; }
    [Required, StringLength(300)] public string Subject { get; set; } = string.Empty;
    [Required, StringLength(4000)] public string Message { get; set; } = string.Empty;
}
