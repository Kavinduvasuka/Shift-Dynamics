namespace ShiftDynamics.API.Domain.Entities;

public enum StockMovementType
{
    Receive,
    Release,
    Adjustment,
    Return
}

public class StockMovement
{
    public Guid Id { get; set; }
    public Guid PartId { get; set; }
    public Guid? RequisitionId { get; set; }
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public Guid PerformedByUserId { get; set; }
    public string? Reference { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Part Part { get; set; } = null!;
}
