namespace ShiftDynamics.API.Application.Interfaces;

public interface IDatabaseService
{
    Task<object> GetHealthAsync(CancellationToken cancellationToken = default);
}

