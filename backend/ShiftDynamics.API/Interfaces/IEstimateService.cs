using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Interfaces;

public interface IEstimateService
{
    Task<IReadOnlyList<Estimate>> ListAsync(Guid? customerId, Guid? workOrderId, EstimateStatus? status);
    Task<Estimate> CreateAsync(Guid workOrderId, decimal laborCost, decimal partsCost, decimal taxAmount, decimal discountAmount, string? notes);
    Task<Estimate?> SendAsync(Guid estimateId);
    Task<Estimate?> DecideAsync(Guid customerId, Guid estimateId, bool approve, string? comment);
}
