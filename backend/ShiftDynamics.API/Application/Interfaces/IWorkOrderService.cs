using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.Interfaces;

public interface IWorkOrderService
{
    Task<IReadOnlyList<WorkOrder>> ListAsync(Guid? customerId, WorkOrderStatus? status);
    Task<WorkOrder?> GetByIdAsync(Guid id, Guid? customerId);
    Task<WorkOrder> CreateAsync(Guid customerId, Guid vehicleId, Guid serviceId, Guid? appointmentId, string? description);
    Task<WorkOrder?> UpdateStatusAsync(Guid id, WorkOrderStatus status, string? notes);
}

