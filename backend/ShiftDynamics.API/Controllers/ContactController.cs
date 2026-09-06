using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/contact-inquiries")]
public class ContactController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public ContactController(ShiftDynamicsDbContext db) => _db = db;

    public class CreateInquiryRequest
    {
        [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [StringLength(30)] public string? Phone { get; set; }
        [StringLength(50)] public string? Type { get; set; }
        [Required, StringLength(300)] public string Subject { get; set; } = string.Empty;
        [Required, StringLength(4000)] public string Message { get; set; } = string.Empty;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateInquiryRequest request)
    {
        var inquiry = new ContactInquiry
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone,
            Type = request.Type,
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            Status = InquiryStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        _db.ContactInquiries.Add(inquiry);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { inquiry.Id }, "Message received."));
    }

    [HttpGet]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] InquiryStatus? status)
    {
        var query = _db.ContactInquiries.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(i => i.Status == status.Value);

        var items = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(Guid id, [FromQuery] InquiryStatus status)
    {
        var inquiry = await _db.ContactInquiries.FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new NotFoundException("Inquiry not found.");

        inquiry.Status = status;
        if (status == InquiryStatus.Read && inquiry.ReadAt is null)
            inquiry.ReadAt = DateTime.UtcNow;
        if (status == InquiryStatus.Resolved)
            inquiry.ResolvedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(inquiry));
    }
}
