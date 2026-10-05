namespace ShiftDynamics.API.Interfaces;

public interface IReportService
{
    Task<object> GetOperationalSummaryAsync(DateTime? from, DateTime? to);
}
