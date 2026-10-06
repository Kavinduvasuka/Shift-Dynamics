using System.ComponentModel.DataAnnotations;

namespace ShiftDynamics.API.Application.DTOs.Procurement;

public record CreateQuoteRequestDto(Guid? PartId, Guid? PartRequisitionId, [Required, StringLength(500)] string PartDescription, [Range(1, int.MaxValue)] int Quantity, DateTime RequiredBy);
public record SubmitQuoteDto([Range(typeof(decimal), "0.01", "999999999")] decimal UnitPrice, [Range(1, int.MaxValue)] int AvailableQuantity, [Range(0, 365)] int DeliveryDays, string? Notes);
public record DeliveryUpdate(DateTime? ExpectedDeliveryAt);
public record ReceiveStock([Range(1, int.MaxValue)] int QuantityReceived);
