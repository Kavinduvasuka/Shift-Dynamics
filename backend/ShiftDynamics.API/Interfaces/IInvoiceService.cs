using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Interfaces;

public interface IInvoiceService
{
    Task<IReadOnlyList<Invoice>> ListAsync(Guid? customerId, InvoiceStatus? status);
    Task<Invoice> CreateAsync(Guid workOrderId, Guid? estimateId, decimal laborCost, decimal partsCost, decimal taxAmount, decimal discountAmount, string? notes);
    Task<Invoice?> ApproveAsync(Guid invoiceId);
}
