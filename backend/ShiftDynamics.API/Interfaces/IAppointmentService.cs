using ShiftDynamics.API.DTOs.Appointments;

namespace ShiftDynamics.API.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentResponse>> GetAllAsync(Guid customerId);
    Task<AppointmentResponse?> GetByIdAsync(Guid id, Guid customerId);
    Task<AppointmentResponse> CreateAsync(Guid customerId, CreateAppointmentRequest request);
    Task<AppointmentResponse?> UpdateAsync(Guid id, Guid customerId, UpdateAppointmentRequest request);
    Task<bool> CancelAsync(Guid id, Guid customerId);
    Task<AppointmentResponse?> ConfirmAsync(Guid id);
}
