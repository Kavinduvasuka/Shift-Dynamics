namespace ShiftDynamics.API.Application.DTOs.Estimates;

public record CreateEstimateRequest(Guid WorkOrderId, decimal LaborCost, decimal PartsCost, decimal TaxAmount = 0, decimal DiscountAmount = 0, string? Notes = null);
public record DecisionRequest(bool Approve, string? Comment);
