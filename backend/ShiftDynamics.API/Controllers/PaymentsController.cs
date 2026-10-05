using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _payments;

    public PaymentsController(IPaymentService payments) => _payments = payments;

    public record CreatePaymentRequest(Guid InvoiceId, decimal Amount, PaymentMethod Method, string? TransactionReference, string? Notes);

    [HttpPost]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreatePaymentRequest request)
    {
        var customerId = User.RequireCustomerId();
        var payment = await _payments.CreateAsync(customerId, request.InvoiceId, request.Amount, request.Method, request.TransactionReference, request.Notes);

        return Ok(ApiResponse<object>.Ok(payment, "Payment recorded."));
    }

    [HttpGet]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] Guid? invoiceId)
    {
        var customerId = User.RequireCustomerId();
        return Ok(ApiResponse<object>.Ok(await _payments.ListForCustomerAsync(customerId, invoiceId)));
    }
}

