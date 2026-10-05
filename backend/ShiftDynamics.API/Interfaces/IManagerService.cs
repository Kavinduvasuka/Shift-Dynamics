using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Interfaces;

public interface IManagerService
{
    Task<object> GetDashboardAsync();
    Task<IReadOnlyList<WorkshopBay>> GetBaysAsync();
    Task<WorkshopBay> CreateBayAsync(string name, BayStatus status, string? notes);
    Task<JobAssignment> AssignJobAsync(Guid workOrderId, Guid mechanicStaffId, Guid? bayId, Guid assignedByUserId);
    Task<object> GetMechanicsAsync();
}
