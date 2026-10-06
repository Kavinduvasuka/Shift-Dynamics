using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using System.Security.Claims;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Application.Interfaces;
using ShiftDynamics.API.Application.DTOs.Inventory;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Policy = "Storekeeper")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventory;

    public InventoryController(IInventoryService inventory) => _inventory = inventory;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] string? search, [FromQuery] bool lowStockOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        if (page < 1 || pageSize is < 1 or > 100) throw new ValidationException("Page must be at least 1 and page size must be between 1 and 100.");
        var items = (await _inventory.ListAsync(search, lowStockOnly))
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
            }).Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var total = (await _inventory.ListAsync(search, lowStockOnly)).Count;
        return Ok(ApiResponse<object>.Ok(new PagedResult<object> { Items = items, Page = page, PageSize = pageSize, TotalCount = total }));
    }

    [HttpPost("parts")]
    public async Task<ActionResult<ApiResponse<object>>> CreatePart([FromBody] UpsertPartRequest request)
    {
        var (part, inventory) = await _inventory.CreatePartAsync(request.PartNumber, request.Name, request.Description, request.Category, request.Compatibility, request.OnHandQty, request.ReorderLevel, request.UnitCost, request.Location);

        return Ok(ApiResponse<object>.Ok(new { part, inventory }, "Part created."));
    }

    [HttpGet("requisitions")]
    public async Task<ActionResult<ApiResponse<object>>> Requisitions([FromQuery] RequisitionStatus? status)
    {
        return Ok(ApiResponse<object>.Ok(await _inventory.ListRequisitionsAsync(status)));
    }

    [HttpPost("requisitions/{id:guid}/review")]
    public async Task<ActionResult<ApiResponse<object>>> Review(Guid id, [FromBody] ReviewRequisitionRequest request)
    {
        var req = await _inventory.ReviewAsync(id, request.Approve, request.Notes, User.RequireUserId()) ?? throw new NotFoundException("Requisition not found.");
        return Ok(ApiResponse<object>.Ok(req, request.Approve ? "Approved." : "Rejected."));
    }

    [HttpPost("requisitions/{id:guid}/release")]
    public async Task<ActionResult<ApiResponse<object>>> Release(Guid id)
    {
        var req = await _inventory.ReleaseAsync(id, User.RequireUserId()) ?? throw new NotFoundException("Requisition not found.");

        return Ok(ApiResponse<object>.Ok(req, "Stock released."));
    }
}

