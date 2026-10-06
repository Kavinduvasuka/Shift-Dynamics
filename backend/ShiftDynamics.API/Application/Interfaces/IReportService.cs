namespace ShiftDynamics.API.Application.Interfaces;

using ShiftDynamics.API.Application.DTOs.Reports;

public interface IReportService
{
    Task<OperationalSummaryResponse> GetOperationalSummaryAsync(DateTime? from, DateTime? to);
}

