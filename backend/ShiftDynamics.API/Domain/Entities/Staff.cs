namespace ShiftDynamics.API.Domain.Entities;

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

    public SystemRole Role { get; set; } = SystemRole.Mechanic;

    public string? Specialization { get; set; }

    public StaffStatus Status { get; set; } = StaffStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
