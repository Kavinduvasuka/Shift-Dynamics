namespace ShiftDynamics.API.Domain.Entities;

public enum BayStatus
{
    Available,
    Occupied,
    Maintenance
}

public class WorkshopBay
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BayStatus Status { get; set; } = BayStatus.Available;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
