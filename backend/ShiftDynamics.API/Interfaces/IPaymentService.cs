using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Interfaces;

public interface IPaymentService
{
    Task<Payment> CreateAsync(Guid customerId, Guid invoiceId, decimal amount, PaymentMethod method, string? transactionReference, string? notes);
    Task<IReadOnlyList<Payment>> ListForCustomerAsync(Guid customerId, Guid? invoiceId);
}
