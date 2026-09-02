namespace ShiftDynamics.API.Domain.Entities;

/// <summary>
/// System-wide roles used for authorization.
/// Matches frontend dashboards: Customer, ServiceAdvisor, Manager, Mechanic, Storekeeper, Vendor, Admin.
/// </summary>
public enum SystemRole
{
    Customer = 0,
    ServiceAdvisor = 1,
    Manager = 2,
    Mechanic = 3,
    Storekeeper = 4,
    Vendor = 5,
    Admin = 6
}

public enum AccountStatus
{
    Active = 0,
    Inactive = 1,
    Pending = 2,
    Rejected = 3
}

public class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public SystemRole Role { get; set; } = SystemRole.Customer;

    public AccountStatus Status { get; set; } = AccountStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public Staff? StaffProfile { get; set; }
}
