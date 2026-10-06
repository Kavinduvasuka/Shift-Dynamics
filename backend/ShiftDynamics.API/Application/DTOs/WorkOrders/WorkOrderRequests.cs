using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.DTOs.WorkOrders;

public record CreateWorkOrderRequest(Guid CustomerId, Guid VehicleId, Guid ServiceId, Guid? AppointmentId, string? Description);
public record UpdateStatusRequest(WorkOrderStatus Status, string? Notes);
