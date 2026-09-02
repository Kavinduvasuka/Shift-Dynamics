using System.ComponentModel.DataAnnotations;

namespace ShiftDynamics.API.DTOs.Auth;

public class RegisterRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(30, MinimumLength = 7)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }
}
