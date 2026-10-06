using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.DTOs.Payments;

public record CreatePaymentRequest(Guid InvoiceId, decimal Amount, PaymentMethod Method, string? TransactionReference, string? Notes);
