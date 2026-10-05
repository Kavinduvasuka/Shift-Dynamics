using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Application.DTOs.Appointments;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ShiftDynamicsDbContext _db;
    public AppointmentService(ShiftDynamicsDbContext db) => _db = db;

    public async Task<IEnumerable<AppointmentResponse>> GetAllAsync(Guid customerId) =>
        (await Query(customerId).AsNoTracking().OrderBy(a => a.AppointmentDate).ToListAsync()).Select(Map);

    public async Task<AppointmentResponse?> GetByIdAsync(Guid id, Guid customerId)
    {
        var appointment = await Query(customerId).AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        return appointment is null ? null : Map(appointment);
    }

    public async Task<AppointmentResponse> CreateAsync(Guid customerId, CreateAppointmentRequest request)
    {
        if (request.AppointmentDate <= DateTime.UtcNow) throw new ValidationException("Appointment date must be in the future.");
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId && v.CustomerId == customerId) ?? throw new NotFoundException("Vehicle not found.");
        var appointment = new Appointment { Id = Guid.NewGuid(), CustomerId = customerId, VehicleId = vehicle.Id, AppointmentDate = request.AppointmentDate, ServiceType = request.ServiceType.Trim(), Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(), Status = AppointmentStatus.Scheduled, CreatedAt = DateTime.UtcNow, Vehicle = vehicle, Customer = (await _db.Customers.FindAsync(customerId))! };
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        return Map(appointment);
    }

    public async Task<AppointmentResponse?> UpdateAsync(Guid id, Guid customerId, UpdateAppointmentRequest request)
    {
        var appointment = await Query(customerId).FirstOrDefaultAsync(a => a.Id == id);
        if (appointment is null) return null;
        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled) throw new ConflictException("Completed or cancelled appointments cannot be changed.");
        if (request.AppointmentDate <= DateTime.UtcNow) throw new ValidationException("Appointment date must be in the future.");
        appointment.AppointmentDate = request.AppointmentDate; appointment.ServiceType = request.ServiceType.Trim(); appointment.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        await _db.SaveChangesAsync();
        return Map(appointment);
    }

    public async Task<bool> CancelAsync(Guid id, Guid customerId)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId);
        if (appointment is null) return false;
        if (appointment.Status != AppointmentStatus.Scheduled) throw new ConflictException("Only scheduled appointments may be cancelled.");
        appointment.Status = AppointmentStatus.Cancelled;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<AppointmentResponse?> ConfirmAsync(Guid id)
    {
        var appointment = await QueryAll().FirstOrDefaultAsync(a => a.Id == id);
        if (appointment is null) return null;
        if (appointment.Status != AppointmentStatus.Scheduled) throw new ConflictException("Only scheduled appointments can be confirmed.");
        appointment.Status = AppointmentStatus.Confirmed;
        await _db.SaveChangesAsync();
        return Map(appointment);
    }

    private IQueryable<Appointment> Query(Guid customerId) => _db.Appointments.Include(a => a.Customer).Include(a => a.Vehicle).Where(a => a.CustomerId == customerId);
    private IQueryable<Appointment> QueryAll() => _db.Appointments.Include(a => a.Customer).Include(a => a.Vehicle);
    private static AppointmentResponse Map(Appointment appointment) => new() { Id = appointment.Id, CustomerId = appointment.CustomerId, VehicleId = appointment.VehicleId, AppointmentDate = appointment.AppointmentDate, ServiceType = appointment.ServiceType, Notes = appointment.Notes, Status = appointment.Status, CreatedAt = appointment.CreatedAt, CustomerName = $"{appointment.Customer.FirstName} {appointment.Customer.LastName}", VehicleRegistration = appointment.Vehicle.RegistrationNumber };
}

