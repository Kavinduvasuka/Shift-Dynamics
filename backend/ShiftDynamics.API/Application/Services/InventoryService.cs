using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly ShiftDynamicsDbContext _db;
    public InventoryService(ShiftDynamicsDbContext db) => _db = db;

    public async Task<IReadOnlyList<InventoryItem>> ListAsync(string? search, bool lowStockOnly)
    {
        var query = _db.InventoryItems.AsNoTracking().Include(i => i.Part).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) { var value = search.Trim().ToLower(); query = query.Where(i => i.Part.Name.ToLower().Contains(value) || i.Part.PartNumber.ToLower().Contains(value) || (i.Part.Category != null && i.Part.Category.ToLower().Contains(value))); }
        if (lowStockOnly) query = query.Where(i => i.OnHandQty <= i.ReorderLevel);
        return await query.OrderBy(i => i.Part.Name).ToListAsync();
    }

    public async Task<(Part Part, InventoryItem Inventory)> CreatePartAsync(string partNumber, string name, string? description, string? category, string? compatibility, int onHandQty, int reorderLevel, decimal unitCost, string? location)
    {
        if (onHandQty < 0 || reorderLevel < 0 || unitCost < 0) throw new ValidationException("Inventory quantities and cost cannot be negative.");
        var normalizedPartNumber = partNumber.Trim();
        if (await _db.Parts.AnyAsync(p => p.PartNumber == normalizedPartNumber)) throw new ConflictException("Part number already exists.");
        var part = new Part { Id = Guid.NewGuid(), PartNumber = normalizedPartNumber, Name = name.Trim(), Description = description?.Trim(), Category = category?.Trim(), Compatibility = compatibility?.Trim(), IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var inventory = new InventoryItem { Id = Guid.NewGuid(), PartId = part.Id, OnHandQty = onHandQty, ReorderLevel = reorderLevel, UnitCost = unitCost, Location = location?.Trim(), UpdatedAt = DateTime.UtcNow };
        _db.Parts.Add(part); _db.InventoryItems.Add(inventory);
        await _db.SaveChangesAsync();
        return (part, inventory);
    }

    public async Task<IReadOnlyList<PartRequisition>> ListRequisitionsAsync(RequisitionStatus? status)
    {
        var query = _db.PartRequisitions.AsNoTracking().Include(r => r.WorkOrder).Include(r => r.RequestedBy).Include(r => r.Part).AsQueryable();
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<PartRequisition?> ReviewAsync(Guid id, bool approve, string? notes, Guid reviewedByUserId)
    {
        var requisition = await _db.PartRequisitions.FirstOrDefaultAsync(r => r.Id == id);
        if (requisition is null) return null;
        if (requisition.Status != RequisitionStatus.Pending) throw new ConflictException("Requisition is not pending.");
        requisition.Status = approve ? RequisitionStatus.Approved : RequisitionStatus.Rejected;
        requisition.ReviewNotes = notes?.Trim(); requisition.ReviewedAt = DateTime.UtcNow; requisition.ReviewedByUserId = reviewedByUserId;
        await _db.SaveChangesAsync();
        return requisition;
    }

    public async Task<PartRequisition?> ReleaseAsync(Guid id, Guid performedByUserId)
    {
        // Integration retry wrapper: ReleaseAsync
        return await _db.Database.CreateExecutionStrategy().ExecuteAsync<PartRequisition?>(async () =>
        {
        _db.ChangeTracker.Clear();
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var requisition = await _db.PartRequisitions.Include(r => r.Part).FirstOrDefaultAsync(r => r.Id == id);
        if (requisition is null) return null;
        if (requisition.Status != RequisitionStatus.Approved) throw new ConflictException("Only approved requisitions can be released.");
        if (requisition.PartId is null) throw new ValidationException("Requisition has no linked part for stock release.");
        var inventory = await _db.InventoryItems.FirstOrDefaultAsync(i => i.PartId == requisition.PartId) ?? throw new NotFoundException("Inventory item not found.");
        var outstanding = requisition.QtyRequested - requisition.QtyReleased;
        if (outstanding <= 0) throw new ConflictException("This requisition has already been fully released.");
        if (inventory.OnHandQty < outstanding) throw new ConflictException($"Insufficient stock. On hand: {inventory.OnHandQty}, requested: {outstanding}.");
        inventory.OnHandQty -= outstanding; inventory.UpdatedAt = DateTime.UtcNow;
        requisition.QtyReleased += outstanding; requisition.Status = RequisitionStatus.Released;
        _db.StockMovements.Add(new StockMovement { Id = Guid.NewGuid(), PartId = requisition.PartId.Value, RequisitionId = requisition.Id, Type = StockMovementType.Release, Quantity = outstanding, PerformedByUserId = performedByUserId, Reference = requisition.Id.ToString(), CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return requisition;
    
        });
    }
}