namespace ShiftDynamics.API.Interfaces;

public interface IDatabaseService
{
    Task<object> GetHealthAsync(CancellationToken cancellationToken = default);
}
