using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

using ShiftDynamics.API.Application.DTOs.Reports;

public class ReportService : IReportService
{
    private readonly ShiftDynamicsDbContext _db;
    public ReportService(ShiftDynamicsDbContext db) => _db = db;
    public async Task<OperationalSummaryResponse> GetOperationalSummaryAsync(DateTime? from, DateTime? to)
    {
        var start = from?.ToUniversalTime() ?? DateTime.UtcNow.Date.AddDays(-30);
        var end = to?.ToUniversalTime() ?? DateTime.UtcNow;
        if (start > end) throw new Common.ValidationException("The report start date must precede the end date.");
        var jobs = _db.WorkOrders.AsNoTracking().Where(w => w.CreatedAt >= start && w.CreatedAt <= end);
        var invoices = _db.Invoices.AsNoTracking().Where(i => i.CreatedAt >= start && i.CreatedAt <= end);
        var payments = _db.Payments.AsNoTracking().Where(p => p.PaymentDate >= start && p.PaymentDate <= end && p.Status == PaymentStatus.Completed);
        return new OperationalSummaryResponse(
            start,
            end,
            new WorkOrderSummary(await jobs.CountAsync(), await jobs.CountAsync(w => w.Status == WorkOrderStatus.Completed), await jobs.CountAsync(w => w.Status == WorkOrderStatus.Cancelled)),
            new BillingSummary(await invoices.CountAsync(), await invoices.SumAsync(i => (decimal?)i.TotalAmount) ?? 0, await payments.SumAsync(p => (decimal?)p.Amount) ?? 0, await invoices.SumAsync(i => (decimal?)i.BalanceDue) ?? 0),
            new InventorySummary(await _db.InventoryItems.CountAsync(i => i.OnHandQty <= i.ReorderLevel), await _db.PartRequisitions.CountAsync(r => r.Status == RequisitionStatus.Pending)),
            new ProcurementSummary(await _db.QuoteRequests.CountAsync(r => r.Status == QuoteRequestStatus.Open), await _db.PurchaseOrders.CountAsync(p => p.Status == PurchaseOrderStatus.Approved || p.Status == PurchaseOrderStatus.Sent || p.Status == PurchaseOrderStatus.PartiallyReceived)));
    }
}

