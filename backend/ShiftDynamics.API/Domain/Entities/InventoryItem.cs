namespace ShiftDynamics.API.Domain.Entities;

public class InventoryItem
{
    public Guid Id { get; set; }
    public Guid PartId { get; set; }
    public int OnHandQty { get; set; }
    public int ReservedQty { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitCost { get; set; }
    public string? Location { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Part Part { get; set; } = null!;
}
