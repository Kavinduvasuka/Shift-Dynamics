using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.Interfaces;

public interface IInventoryService
{
    Task<IReadOnlyList<InventoryItem>> ListAsync(string? search, bool lowStockOnly);
    Task<(Part Part, InventoryItem Inventory)> CreatePartAsync(string partNumber, string name, string? description, string? category, string? compatibility, int onHandQty, int reorderLevel, decimal unitCost, string? location);
    Task<IReadOnlyList<PartRequisition>> ListRequisitionsAsync(RequisitionStatus? status);
    Task<PartRequisition?> ReviewAsync(Guid id, bool approve, string? notes, Guid reviewedByUserId);
    Task<PartRequisition?> ReleaseAsync(Guid id, Guid performedByUserId);
}

