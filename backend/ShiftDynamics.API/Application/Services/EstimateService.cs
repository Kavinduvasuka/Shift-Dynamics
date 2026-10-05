using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class EstimateService : IEstimateService
{
    private readonly ShiftDynamicsDbContext _db;
    private readonly INotificationService _notifications;
    public EstimateService(ShiftDynamicsDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<IReadOnlyList<Estimate>> ListAsync(Guid? customerId, Guid? workOrderId, EstimateStatus? status)
    {
        var query = _db.Estimates.AsNoTracking().Include(e => e.WorkOrder).AsQueryable();
        if (customerId.HasValue) query = query.Where(e => e.WorkOrder.CustomerId == customerId.Value);
        if (workOrderId.HasValue) query = query.Where(e => e.WorkOrderId == workOrderId.Value);
        if (status.HasValue) query = query.Where(e => e.Status == status.Value);
        return await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
    }

    public async Task<Estimate> CreateAsync(Guid workOrderId, decimal laborCost, decimal partsCost, decimal taxAmount, decimal discountAmount, string? notes)
    {
        if (laborCost < 0 || partsCost < 0 || taxAmount < 0 || discountAmount < 0) throw new ValidationException("Estimate amounts cannot be negative.");
        if (!await _db.WorkOrders.AnyAsync(w => w.Id == workOrderId)) throw new NotFoundException("Work order not found.");
        var subtotal = laborCost + partsCost;
        var total = subtotal + taxAmount - discountAmount;
        if (total < 0) throw new ValidationException("Discount cannot exceed the estimate subtotal plus tax.");
        var sequence = await _db.Estimates.CountAsync() + 1;
        var estimate = new Estimate { Id = Guid.NewGuid(), WorkOrderId = workOrderId, EstimateNumber = $"EST-{DateTime.UtcNow:yyyyMMdd}-{sequence:D4}", LaborCost = laborCost, PartsCost = partsCost, Subtotal = subtotal, TaxAmount = taxAmount, DiscountAmount = discountAmount, TotalAmount = total, Status = EstimateStatus.Draft, Notes = notes?.Trim(), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _db.Estimates.Add(estimate);
        await _db.SaveChangesAsync();
        return estimate;
    }

    public async Task<Estimate?> SendAsync(Guid estimateId)
    {
        var estimate = await _db.Estimates.Include(e => e.WorkOrder).FirstOrDefaultAsync(e => e.Id == estimateId);
        if (estimate is null) return null;
        if (estimate.Status != EstimateStatus.Draft) throw new ConflictException("Only draft estimates can be sent.");
        estimate.Status = EstimateStatus.Sent;
        estimate.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        var customerUserId = await _db.Users.Where(u => u.CustomerId == estimate.WorkOrder.CustomerId).Select(u => (Guid?)u.Id).FirstOrDefaultAsync();
        if (customerUserId.HasValue) await _notifications.CreateAsync(customerUserId.Value, "estimate", "Estimate ready for review", $"Estimate {estimate.EstimateNumber} is ready for your decision.", "Estimate", estimate.Id);
        return estimate;
    }

    public async Task<Estimate?> DecideAsync(Guid customerId, Guid estimateId, bool approve, string? comment)
    {
        var estimate = await _db.Estimates.Include(e => e.WorkOrder).FirstOrDefaultAsync(e => e.Id == estimateId && e.WorkOrder.CustomerId == customerId);
        if (estimate is null) return null;
        if (estimate.Status != EstimateStatus.Sent) throw new ConflictException("Estimate is not awaiting decision.");
        estimate.Status = approve ? EstimateStatus.Approved : EstimateStatus.Rejected;
        estimate.CustomerApproved = approve;
        estimate.ApprovedAt = approve ? DateTime.UtcNow : null;
        estimate.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(comment)) estimate.Notes = (estimate.Notes + " | Customer: " + comment.Trim()).Trim();
        await _db.SaveChangesAsync();
        return estimate;
    }
}

