namespace ShiftDynamics.API.Application.DTOs.ServiceHistory;

public record ServiceHistoryResponse(Guid Id, string WorkOrderNumber, DateTime? CompletedAt, string? Description, string? TechnicianNotes, VehicleHistoryResponse Vehicle, ServiceHistoryItemResponse Service);
public record VehicleHistoryResponse(Guid Id, string RegistrationNumber, string Make, string Model);
public record ServiceHistoryItemResponse(Guid Id, string Name);
