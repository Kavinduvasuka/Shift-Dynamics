namespace ShiftDynamics.API.Interfaces;

public interface IHealthService
{
    object GetLiveness(string environment);
    Task<object> GetReadinessAsync(CancellationToken cancellationToken = default);
}
