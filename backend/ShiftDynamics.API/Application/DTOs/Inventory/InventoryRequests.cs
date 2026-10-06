namespace ShiftDynamics.API.Application.DTOs.Inventory;

public record UpsertPartRequest(string PartNumber, string Name, string? Description, string? Category, string? Compatibility, int OnHandQty, int ReorderLevel, decimal UnitCost, string? Location);
public record ReviewRequisitionRequest(bool Approve, string? Notes);
