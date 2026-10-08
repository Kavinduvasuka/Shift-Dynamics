namespace ShiftDynamics.API.Application.DTOs.ServiceHistory;

public record ServiceHistoryResponse(
    Guid Id,
    string WorkOrderNumber,
    DateTime? CompletedAt,
    string? Description,
    string? TechnicianNotes,
    VehicleHistoryResponse Vehicle,
    ServiceHistoryItemResponse Service)
{
    public IReadOnlyList<DiagnosticHistoryResponse> Diagnostics { get; set; }
        = Array.Empty<DiagnosticHistoryResponse>();

    public IReadOnlyList<RepairHistoryResponse> Repairs { get; set; }
        = Array.Empty<RepairHistoryResponse>();

    public IReadOnlyList<RecommendationHistoryResponse> Recommendations { get; set; }
        = Array.Empty<RecommendationHistoryResponse>();
}

public record VehicleHistoryResponse(
    Guid Id, string RegistrationNumber, string Make, string Model);

public record ServiceHistoryItemResponse(Guid Id, string Name);

public record DiagnosticHistoryResponse(
    Guid Id,
    Guid WorkOrderId,
    string Finding,
    string? Severity,
    DateTime CreatedAt);

public record RepairHistoryResponse(
    Guid Id,
    Guid WorkOrderId,
    string Action,
    string? Notes,
    DateTime CreatedAt);

public record RecommendationHistoryResponse(
    Guid Id,
    Guid WorkOrderId,
    string Recommendation,
    string? Priority,
    DateTime CreatedAt);