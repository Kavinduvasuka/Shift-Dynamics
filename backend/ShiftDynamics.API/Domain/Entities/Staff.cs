namespace ShiftDynamics.API.Domain.Entities;

public enum StaffRole
{
    Mechanic,
    ServiceAdvisor,
    Manager,
    Admin
}

public enum StaffStatus
{
    Active,
    Inactive,
    OnLeave
}

public class Staff
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;

    public StaffRole Role { get; set; } = StaffRole.Mechanic;

    public string? Specialization { get; set; }

    public StaffStatus Status { get; set; } = StaffStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
