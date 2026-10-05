namespace ShiftDynamics.API.Application.Interfaces;

public interface IReportService
{
    Task<object> GetOperationalSummaryAsync(DateTime? from, DateTime? to);
}

