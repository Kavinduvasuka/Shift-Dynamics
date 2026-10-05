using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/procurement")]
[Authorize]
public class ProcurementController : ControllerBase
{
    private readonly IProcurementService _procurement;
    public ProcurementController(IProcurementService procurement) => _procurement = procurement;
    public record CreateQuoteRequestDto(Guid? PartId, Guid? PartRequisitionId, [Required, StringLength(500)] string PartDescription, [Range(1, int.MaxValue)] int Quantity, DateTime RequiredBy);
    public record SubmitQuoteDto([Range(typeof(decimal), "0.01", "999999999")] decimal UnitPrice, [Range(1, int.MaxValue)] int AvailableQuantity, [Range(0, 365)] int DeliveryDays, string? Notes);
    public record DeliveryUpdate(DateTime? ExpectedDeliveryAt);
    public record ReceiveStock([Range(1, int.MaxValue)] int QuantityReceived);

    [HttpPost("quote-requests")]
    [Authorize(Policy = "Storekeeper")]
    public async Task<ActionResult<ApiResponse<object>>> CreateQuoteRequest(CreateQuoteRequestDto request)
    {
        var item = await _procurement.CreateRequestAsync(User.RequireUserId(), request.PartId, request.PartRequisitionId, request.PartDescription, request.Quantity, request.RequiredBy);
        return Ok(ApiResponse<object>.Ok(item, "Quote request created."));
    }

    [HttpGet("quote-requests")]
    [Authorize(Policy = "Staff")]
    public async Task<ActionResult<ApiResponse<object>>> ListQuoteRequests([FromQuery] QuoteRequestStatus? status) =>
        Ok(ApiResponse<object>.Ok(await _procurement.ListRequestsAsync(status)));

    [HttpGet("vendor/quote-requests")]
    [Authorize(Policy = "Vendor")]
    public async Task<ActionResult<ApiResponse<object>>> ListVendorQuoteRequests([FromQuery] QuoteRequestStatus? status)
    {
        var vendorId = User.RequireUserId();
        return Ok(ApiResponse<object>.Ok(await _procurement.ListRequestsAsync(status, null)));
    }

    [HttpPost("quote-requests/{id:guid}/quotes")]
    [Authorize(Policy = "Vendor")]
    public async Task<ActionResult<ApiResponse<object>>> SubmitQuote(Guid id, SubmitQuoteDto request)
    {
        var quote = await _procurement.SubmitQuoteAsync(User.RequireUserId(), id, request.UnitPrice, request.AvailableQuantity, request.DeliveryDays, request.Notes);
        return Ok(ApiResponse<object>.Ok(quote, "Quote submitted."));
    }

    [HttpPost("quotes/{id:guid}/award")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> Award(Guid id)
    {
        var order = await _procurement.AwardAsync(User.RequireUserId(), id);
        return Ok(ApiResponse<object>.Ok(order, "Quote awarded and purchase order created."));
    }

    [HttpPatch("purchase-orders/{id:guid}/delivery")]
    [Authorize(Policy = "Vendor")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateDelivery(Guid id, DeliveryUpdate request)
    {
        var order = await _procurement.UpdateDeliveryAsync(User.RequireUserId(), id, request.ExpectedDeliveryAt);
        return order is null ? NotFound() : Ok(ApiResponse<object>.Ok(order));
    }

    [HttpPost("purchase-orders/{id:guid}/receive")]
    [Authorize(Policy = "Storekeeper")]
    public async Task<ActionResult<ApiResponse<object>>> Receive(Guid id, ReceiveStock request)
    {
        var order = await _procurement.ReceiveAsync(User.RequireUserId(), id, request.QuantityReceived);
        return order is null ? NotFound() : Ok(ApiResponse<object>.Ok(order, "Stock received."));
    }
}

