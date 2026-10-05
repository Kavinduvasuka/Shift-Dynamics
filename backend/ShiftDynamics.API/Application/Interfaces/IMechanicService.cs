using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.Interfaces;

public interface IMechanicService
{
    Task<Guid> GetCurrentMechanicIdAsync(Guid userId);
    Task<LaborSession> StartTimerAsync(Guid mechanicId, Guid workOrderId);
    Task<LaborSession> EndTimerAsync(Guid mechanicId, Guid workOrderId);
    Task<DiagnosticFinding> AddDiagnosticAsync(Guid mechanicId, Guid workOrderId, string finding, string? severity);
    Task<RepairAction> AddRepairAsync(Guid mechanicId, Guid workOrderId, string action, string? notes);
    Task<MechanicRecommendation> AddRecommendationAsync(Guid mechanicId, Guid workOrderId, string recommendation, string? priority);
    Task<PartRequisition> CreateRequisitionAsync(Guid mechanicId, Guid workOrderId, Guid? partId, string partSpec, int quantity, RequisitionUrgency urgency, string? reason);
    Task<WorkOrder> UpdateJobStatusAsync(Guid mechanicId, Guid workOrderId, WorkOrderStatus status, string? notes);
    Task<WorkOrder> CompleteJobAsync(Guid mechanicId, Guid workOrderId, string? notes = null);
}

