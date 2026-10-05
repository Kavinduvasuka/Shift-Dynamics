using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.Interfaces;

public interface IProcurementService
{
    Task<QuoteRequest> CreateRequestAsync(Guid createdByUserId, Guid? partId, Guid? requisitionId, string partDescription, int quantity, DateTime requiredBy);
    Task<IReadOnlyList<QuoteRequest>> ListRequestsAsync(QuoteRequestStatus? status, Guid? vendorProfileId = null);
    Task<VendorQuote> SubmitQuoteAsync(Guid vendorUserId, Guid quoteRequestId, decimal unitPrice, int availableQuantity, int deliveryDays, string? notes);
    Task<PurchaseOrder> AwardAsync(Guid managerUserId, Guid vendorQuoteId);
    Task<PurchaseOrder?> UpdateDeliveryAsync(Guid vendorUserId, Guid purchaseOrderId, DateTime? expectedDeliveryAt);
    Task<PurchaseOrder?> ReceiveAsync(Guid storekeeperUserId, Guid purchaseOrderId, int quantityReceived);
}

