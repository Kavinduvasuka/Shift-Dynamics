using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Interfaces;

namespace ShiftDynamics.API.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly ShiftDynamicsDbContext _db;
    public WorkOrderService(ShiftDynamicsDbContext db) => _db = db;

    public async Task<IReadOnlyList<WorkOrder>> ListAsync(Guid? customerId, WorkOrderStatus? status)
    {
        var query = IncludeDetails(_db.WorkOrders.AsNoTracking());
        if (customerId.HasValue) query = query.Where(w => w.CustomerId == customerId.Value);
        if (status.HasValue) query = query.Where(w => w.Status == status.Value);
        return await query.OrderByDescending(w => w.CreatedAt).ToListAsync();
    }

    public async Task<WorkOrder?> GetByIdAsync(Guid id, Guid? customerId)
    {
        var query = IncludeDetails(_db.WorkOrders.AsNoTracking()).Where(w => w.Id == id);
        if (customerId.HasValue) query = query.Where(w => w.CustomerId == customerId.Value);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<WorkOrder> CreateAsync(Guid customerId, Guid vehicleId, Guid serviceId, Guid? appointmentId, string? description)
    {
        if (!await _db.Customers.AnyAsync(c => c.Id == customerId)) throw new NotFoundException("Customer not found.");
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId) ?? throw new NotFoundException("Vehicle not found.");
        if (vehicle.CustomerId != customerId) throw new ValidationException("Vehicle does not belong to the specified customer.");
        if (!await _db.Services.AnyAsync(s => s.Id == serviceId && s.IsActive)) throw new NotFoundException("Service not found or inactive.");
        if (appointmentId.HasValue && !await _db.Appointments.AnyAsync(a => a.Id == appointmentId && a.CustomerId == customerId && a.VehicleId == vehicleId && a.Status == AppointmentStatus.Confirmed)) throw new ValidationException("Appointment must be confirmed and belong to the selected customer and vehicle.");
        var sequence = await _db.WorkOrders.CountAsync() + 1;
        var workOrder = new WorkOrder { Id = Guid.NewGuid(), WorkOrderNumber = $"WO-{DateTime.UtcNow:yyyyMMdd}-{sequence:D4}", CustomerId = customerId, VehicleId = vehicleId, ServiceId = serviceId, AppointmentId = appointmentId, Description = description?.Trim(), Status = WorkOrderStatus.Open, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _db.WorkOrders.Add(workOrder);
        await _db.SaveChangesAsync();
        return workOrder;
    }

    public async Task<WorkOrder?> UpdateStatusAsync(Guid id, WorkOrderStatus status, string? notes)
    {
        var workOrder = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == id);
        if (workOrder is null) return null;
        if (!IsValidTransition(workOrder.Status, status)) throw new ConflictException($"Cannot change a work order from {workOrder.Status} to {status}.");
        workOrder.Status = status;
        if (!string.IsNullOrWhiteSpace(notes)) workOrder.TechnicianNotes = notes.Trim();
        if (status == WorkOrderStatus.InProgress && workOrder.StartedAt is null) workOrder.StartedAt = DateTime.UtcNow;
        if (status == WorkOrderStatus.Completed) workOrder.CompletedAt = DateTime.UtcNow;
        workOrder.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return workOrder;
    }

    private static IQueryable<WorkOrder> IncludeDetails(IQueryable<WorkOrder> query) => query.Include(w => w.Customer).Include(w => w.Vehicle).Include(w => w.Service).Include(w => w.AssignedStaff);
    private static bool IsValidTransition(WorkOrderStatus current, WorkOrderStatus next) => current == next || (current, next) switch
    {
        (WorkOrderStatus.Open, WorkOrderStatus.Assigned or WorkOrderStatus.Cancelled) => true,
        (WorkOrderStatus.Assigned, WorkOrderStatus.InProgress or WorkOrderStatus.WaitingForParts or WorkOrderStatus.Cancelled) => true,
        (WorkOrderStatus.InProgress, WorkOrderStatus.WaitingForParts or WorkOrderStatus.Completed) => true,
        (WorkOrderStatus.WaitingForParts, WorkOrderStatus.InProgress or WorkOrderStatus.Cancelled) => true,
        _ => false
    };
}
