using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.DTOs.Manager;

public record UpsertBayRequest(string Name, BayStatus Status, string? Notes);
public record AssignJobRequest(Guid WorkOrderId, Guid MechanicStaffId, Guid? BayId);
