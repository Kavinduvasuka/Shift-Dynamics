using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Application.Interfaces;
using ShiftDynamics.API.Application.DTOs.Contact;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/contact-inquiries")]
public class ContactController : ControllerBase
{
    private readonly IContactService _contact;

    public ContactController(IContactService contact) => _contact = contact;

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateInquiryRequest request)
    {
        var inquiry = await _contact.CreateAsync(request.Name, request.Email, request.Phone, request.Type, request.Subject, request.Message);

        return Ok(ApiResponse<object>.Ok(new { inquiry.Id }, "Message received."));
    }

    [HttpGet]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] InquiryStatus? status)
    {
        return Ok(ApiResponse<object>.Ok(await _contact.ListAsync(status)));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(Guid id, [FromQuery] InquiryStatus status)
    {
        var inquiry = await _contact.UpdateStatusAsync(id, status) ?? throw new NotFoundException("Inquiry not found.");
        return Ok(ApiResponse<object>.Ok(inquiry));
    }
}

