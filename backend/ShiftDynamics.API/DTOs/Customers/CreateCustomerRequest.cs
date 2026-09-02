using System.ComponentModel.DataAnnotations;

namespace ShiftDynamics.API.DTOs.Customers;

public class CreateCustomerRequest
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required.")]
    [StringLength(30, MinimumLength = 7)]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email format is invalid.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
}
