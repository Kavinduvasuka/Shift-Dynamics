using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Application.Interfaces;
using ShiftDynamics.API.Application.DTOs.Invoices;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoices;

    public InvoicesController(IInvoiceService invoices) => _invoices = invoices;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] InvoiceStatus? status)
    {
        var customerId = User.IsInRole(SystemRole.Customer.ToString()) ? User.RequireCustomerId() : (Guid?)null;
        return Ok(ApiResponse<object>.Ok(await _invoices.ListAsync(customerId, status)));
    }

    [HttpPost]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateInvoiceRequest request)
    {
        var invoice = await _invoices.CreateAsync(request.WorkOrderId, request.EstimateId, request.LaborCost, request.PartsCost, request.TaxAmount, request.DiscountAmount, request.Notes);
        return Ok(ApiResponse<object>.Ok(invoice, "Invoice created."));
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> Approve(Guid id)
    {
        var invoice = await _invoices.ApproveAsync(id) ?? throw new NotFoundException("Invoice not found.");
        return Ok(ApiResponse<object>.Ok(invoice, "Invoice approved and issued."));
    }
}

