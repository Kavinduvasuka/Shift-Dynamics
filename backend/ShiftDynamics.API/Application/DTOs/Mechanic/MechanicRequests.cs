using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.DTOs.Mechanic;

public record TimerActionRequest(Guid WorkOrderId);
public record CreateDiagnosticRequest(Guid WorkOrderId, string Finding, string? Severity);
public record CreateRepairRequest(Guid WorkOrderId, string Action, string? Notes);
public record CreateRecommendationRequest(Guid WorkOrderId, string Recommendation, string? Priority);
public record UpdateJobStatusRequest(WorkOrderStatus Status, string? Notes);
public record CreateRequisitionRequest(Guid WorkOrderId, Guid? PartId, string PartSpec, int QtyRequested, RequisitionUrgency Urgency, string? Reason);
