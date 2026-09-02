using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Policy = "Storekeeper")]
public class InventoryController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public InventoryController(ShiftDynamicsDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] string? search, [FromQuery] bool lowStockOnly = false)
    {
        var query = _db.InventoryItems
            .AsNoTracking()
            .Include(i => i.Part)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(i =>
                i.Part.Name.ToLower().Contains(s) ||
                i.Part.PartNumber.ToLower().Contains(s) ||
                (i.Part.Category != null && i.Part.Category.ToLower().Contains(s)));
        }

        if (lowStockOnly)
            query = query.Where(i => i.OnHandQty <= i.ReorderLevel);

        var items = await query
            .OrderBy(i => i.Part.Name)
            .Select(i => new
            {
                i.Id,
                i.PartId,
                i.Part.PartNumber,
                i.Part.Name,
                i.Part.Category,
                i.OnHandQty,
                i.ReservedQty,
                i.ReorderLevel,
                i.UnitCost,
                i.Location,
                IsLowStock = i.OnHandQty <= i.ReorderLevel,
                i.UpdatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    public record UpsertPartRequest(
        string PartNumber, string Name, string? Description, string? Category,
        string? Compatibility, int OnHandQty, int ReorderLevel, decimal UnitCost, string? Location);

    [HttpPost("parts")]
    public async Task<ActionResult<ApiResponse<object>>> CreatePart([FromBody] UpsertPartRequest request)
    {
        if (await _db.Parts.AnyAsync(p => p.PartNumber == request.PartNumber.Trim()))
            throw new ConflictException("Part number already exists.");

        var part = new Part
        {
            Id = Guid.NewGuid(),
            PartNumber = request.PartNumber.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description,
            Category = request.Category,
            Compatibility = request.Compatibility,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var inventory = new InventoryItem
        {
            Id = Guid.NewGuid(),
            PartId = part.Id,
            OnHandQty = request.OnHandQty,
            ReorderLevel = request.ReorderLevel,
            UnitCost = request.UnitCost,
            Location = request.Location,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Parts.Add(part);
        _db.InventoryItems.Add(inventory);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { part, inventory }, "Part created."));
    }

    [HttpGet("requisitions")]
    public async Task<ActionResult<ApiResponse<object>>> Requisitions([FromQuery] RequisitionStatus? status)
    {
        var query = _db.PartRequisitions
            .AsNoTracking()
            .Include(r => r.WorkOrder)
            .Include(r => r.RequestedBy)
            .Include(r => r.Part)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var items = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    public record ReviewRequisitionRequest(bool Approve, string? Notes);

    [HttpPost("requisitions/{id:guid}/review")]
    public async Task<ActionResult<ApiResponse<object>>> Review(Guid id, [FromBody] ReviewRequisitionRequest request)
    {
        var req = await _db.PartRequisitions.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("Requisition not found.");

        if (req.Status != RequisitionStatus.Pending)
            throw new ConflictException("Requisition is not pending.");

        req.Status = request.Approve ? RequisitionStatus.Approved : RequisitionStatus.Rejected;
        req.ReviewNotes = request.Notes;
        req.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(req, request.Approve ? "Approved." : "Rejected."));
    }

    [HttpPost("requisitions/{id:guid}/release")]
    public async Task<ActionResult<ApiResponse<object>>> Release(Guid id)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var req = await _db.PartRequisitions
            .Include(r => r.Part)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("Requisition not found.");

        if (req.Status != RequisitionStatus.Approved)
            throw new ConflictException("Only approved requisitions can be released.");

        if (req.PartId is null)
            throw new ValidationException("Requisition has no linked part for stock release.");

        var inventory = await _db.InventoryItems.FirstOrDefaultAsync(i => i.PartId == req.PartId)
            ?? throw new NotFoundException("Inventory item not found.");

        if (inventory.OnHandQty < req.QtyRequested)
            throw new ConflictException($"Insufficient stock. On hand: {inventory.OnHandQty}, requested: {req.QtyRequested}.");

        inventory.OnHandQty -= req.QtyRequested;
        inventory.UpdatedAt = DateTime.UtcNow;
        req.QtyReleased = req.QtyRequested;
        req.Status = RequisitionStatus.Released;

        _db.StockMovements.Add(new StockMovement
        {
            Id = Guid.NewGuid(),
            PartId = req.PartId.Value,
            RequisitionId = req.Id,
            Type = StockMovementType.Release,
            Quantity = req.QtyRequested,
            PerformedByUserId = Guid.Empty, // filled by auth in production
            Reference = req.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(ApiResponse<object>.Ok(req, "Stock released."));
    }
}
