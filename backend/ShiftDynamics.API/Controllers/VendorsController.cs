using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/vendors")]
public class VendorsController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public VendorsController(ShiftDynamicsDbContext db) => _db = db;

    public class VendorRegisterRequest
    {
        [Required, StringLength(200)] public string BusinessName { get; set; } = string.Empty;
        [Required, StringLength(150)] public string ContactPerson { get; set; } = string.Empty;
        [Required, StringLength(30)] public string Mobile { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [StringLength(500)] public string? Address { get; set; }
        [StringLength(200)] public string? Specialization { get; set; }
        [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = string.Empty;
    }

    [HttpPost("registrations")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] VendorRegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.VendorRegistrations.AnyAsync(v => v.Email == email && v.Status == VendorRegistrationStatus.Pending))
            throw new ConflictException("A pending registration already exists for this email.");

        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new ConflictException("An account with this email already exists.");

        var reg = new VendorRegistration
        {
            Id = Guid.NewGuid(),
            BusinessName = request.BusinessName.Trim(),
            ContactPerson = request.ContactPerson.Trim(),
            Mobile = request.Mobile.Trim(),
            Email = email,
            Address = request.Address,
            Specialization = request.Specialization,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Status = VendorRegistrationStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        _db.VendorRegistrations.Add(reg);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { reg.Id, reg.Status }, "Vendor registration submitted for review."));
    }

    [HttpGet("registrations")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> ListRegistrations([FromQuery] VendorRegistrationStatus? status)
    {
        var query = _db.VendorRegistrations.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(v => v.Status == status.Value);

        var items = await query
            .OrderByDescending(v => v.SubmittedAt)
            .Select(v => new
            {
                v.Id, v.BusinessName, v.ContactPerson, v.Mobile, v.Email,
                v.Address, v.Specialization, v.Status, v.SubmittedAt, v.ReviewedAt, v.RejectionReason
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    public record ReviewVendorRequest(bool Approve, string? RejectionReason);

    [HttpPost("registrations/{id:guid}/review")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> Review(Guid id, [FromBody] ReviewVendorRequest request)
    {
        var reg = await _db.VendorRegistrations.FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new NotFoundException("Registration not found.");

        if (reg.Status != VendorRegistrationStatus.Pending)
            throw new ConflictException("Registration is not pending.");

        reg.ReviewedAt = DateTime.UtcNow;

        if (!request.Approve)
        {
            reg.Status = VendorRegistrationStatus.Rejected;
            reg.RejectionReason = request.RejectionReason;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(reg, "Vendor registration rejected."));
        }

        // Approve: create User account + VendorProfile
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = reg.ContactPerson,
            Email = reg.Email,
            Phone = reg.Mobile,
            PasswordHash = reg.PasswordHash,
            Role = SystemRole.Vendor,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var profile = new VendorProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RegistrationId = reg.Id,
            BusinessName = reg.BusinessName,
            ContactPerson = reg.ContactPerson,
            Mobile = reg.Mobile,
            Email = reg.Email,
            Address = reg.Address,
            Specialization = reg.Specialization,
            ApprovalStatus = VendorApprovalStatus.Active,
            ApprovedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        reg.Status = VendorRegistrationStatus.Approved;
        reg.CreatedUserId = user.Id;

        _db.Users.Add(user);
        _db.VendorProfiles.Add(profile);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { reg.Id, UserId = user.Id, VendorProfileId = profile.Id }, "Vendor approved and account activated."));
    }

    /// <summary>Current vendor's own business profile.</summary>
    [HttpGet("me")]
    [Authorize(Policy = "Vendor")]
    public async Task<ActionResult<ApiResponse<object>>> MyProfile()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException();

        var profile = await _db.VendorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.UserId == userId)
            ?? throw new NotFoundException("Vendor profile not found.");

        return Ok(ApiResponse<object>.Ok(profile));
    }

    /// <summary>Manager: list vendor profiles.</summary>
    [HttpGet("profiles")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> ListProfiles([FromQuery] VendorApprovalStatus? status)
    {
        var query = _db.VendorProfiles.AsNoTracking().AsQueryable();
        if (status.HasValue)
            query = query.Where(v => v.ApprovalStatus == status.Value);

        var items = await query
            .OrderBy(v => v.BusinessName)
            .Select(v => new
            {
                v.Id,
                v.UserId,
                v.BusinessName,
                v.ContactPerson,
                v.Mobile,
                v.Email,
                v.Address,
                v.Specialization,
                v.ApprovalStatus,
                v.ApprovedAt,
                v.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    public record UpdateVendorStatusRequest(VendorApprovalStatus Status);

    /// <summary>Manager: suspend / reactivate a vendor profile.</summary>
    [HttpPatch("profiles/{id:guid}/status")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProfileStatus(Guid id, [FromBody] UpdateVendorStatusRequest request)
    {
        var profile = await _db.VendorProfiles
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new NotFoundException("Vendor profile not found.");

        profile.ApprovalStatus = request.Status;
        profile.UpdatedAt = DateTime.UtcNow;

        profile.User.Status = request.Status switch
        {
            VendorApprovalStatus.Active => AccountStatus.Active,
            VendorApprovalStatus.Suspended => AccountStatus.Inactive,
            VendorApprovalStatus.Inactive => AccountStatus.Inactive,
            _ => profile.User.Status
        };
        profile.User.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(profile, "Vendor status updated."));
    }


    // ---------- Vendor procurement (quotes & POs) ----------

    private async Task<VendorProfile> RequireActiveVendorProfileAsync()
    {
        var userId = User.RequireUserId();
        var profile = await _db.VendorProfiles.FirstOrDefaultAsync(v => v.UserId == userId)
            ?? throw new ForbiddenException("Vendor profile not found.");
        if (profile.ApprovalStatus != VendorApprovalStatus.Active)
            throw new ForbiddenException("Vendor account is not active.");
        return profile;
    }

    [HttpGet("quote-requests")]
    [Authorize(Policy = "Vendor")]
    public async Task<ActionResult<ApiResponse<object>>> OpenQuoteRequests()
    {
        var profile = await RequireActiveVendorProfileAsync();

        var quotedIds = await _db.VendorQuotes
            .Where(q => q.VendorProfileId == profile.Id)
            .Select(q => q.QuoteRequestId)
            .ToListAsync();

        var items = await _db.VendorQuoteRequests.AsNoTracking()
            .Include(r => r.Part)
            .Where(r => r.Status == ProcurementRequestStatus.Open)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.PartId,
                PartNumber = r.Part.PartNumber,
                PartName = r.Part.Name,
                r.Quantity,
                r.Specifications,
                r.Status,
                r.CreatedAt,
                AlreadyQuoted = quotedIds.Contains(r.Id)
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    public record SubmitQuoteDto(decimal UnitPrice, int DeliveryDays, string? Notes);

    [HttpPost("quote-requests/{requestId:guid}/quotes")]
    [Authorize(Policy = "Vendor")]
    public async Task<ActionResult<ApiResponse<object>>> SubmitQuote(Guid requestId, [FromBody] SubmitQuoteDto request)
    {
        if (request.UnitPrice < 0 || request.DeliveryDays < 0)
            throw new Common.ValidationException("Unit price and delivery days must be non-negative.");

        var profile = await RequireActiveVendorProfileAsync();
        var quoteRequest = await _db.VendorQuoteRequests.FirstOrDefaultAsync(r => r.Id == requestId)
            ?? throw new NotFoundException("Quote request not found.");

        if (quoteRequest.Status != ProcurementRequestStatus.Open)
            throw new ConflictException("Quote request is not open.");

        if (await _db.VendorQuotes.AnyAsync(q => q.QuoteRequestId == requestId && q.VendorProfileId == profile.Id))
            throw new ConflictException("You have already submitted a quote for this request.");

        var quote = new VendorQuote
        {
            Id = Guid.NewGuid(),
            QuoteRequestId = requestId,
            VendorProfileId = profile.Id,
            UnitPrice = request.UnitPrice,
            DeliveryDays = request.DeliveryDays,
            Notes = request.Notes?.Trim(),
            Status = VendorQuoteStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };

        _db.VendorQuotes.Add(quote);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            quote.Id,
            quote.QuoteRequestId,
            quote.UnitPrice,
            quote.DeliveryDays,
            quote.Notes,
            quote.Status,
            quote.SubmittedAt
        }, "Quote submitted."));
    }

    [HttpGet("quotes")]
    [Authorize(Policy = "Vendor")]
    public async Task<ActionResult<ApiResponse<object>>> MyQuotes([FromQuery] VendorQuoteStatus? status)
    {
        var profile = await RequireActiveVendorProfileAsync();
        var query = _db.VendorQuotes.AsNoTracking()
            .Include(q => q.QuoteRequest).ThenInclude(r => r.Part)
            .Where(q => q.VendorProfileId == profile.Id);

        if (status.HasValue) query = query.Where(q => q.Status == status.Value);

        var items = await query.OrderByDescending(q => q.SubmittedAt)
            .Select(q => new
            {
                q.Id,
                q.QuoteRequestId,
                PartNumber = q.QuoteRequest.Part.PartNumber,
                PartName = q.QuoteRequest.Part.Name,
                Quantity = q.QuoteRequest.Quantity,
                q.UnitPrice,
                q.DeliveryDays,
                q.Notes,
                q.Status,
                q.SubmittedAt,
                Total = q.UnitPrice * q.QuoteRequest.Quantity
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("purchase-orders")]
    [Authorize(Policy = "Vendor")]
    public async Task<ActionResult<ApiResponse<object>>> MyPurchaseOrders([FromQuery] PurchaseOrderStatus? status)
    {
        var profile = await RequireActiveVendorProfileAsync();
        var query = _db.PurchaseOrders.AsNoTracking()
            .Include(po => po.Part)
            .Where(po => po.VendorProfileId == profile.Id);

        if (status.HasValue) query = query.Where(po => po.Status == status.Value);

        var items = await query.OrderByDescending(po => po.CreatedAt)
            .Select(po => new
            {
                po.Id,
                po.OrderNumber,
                PartNumber = po.Part.PartNumber,
                PartName = po.Part.Name,
                po.Quantity,
                po.UnitPrice,
                po.TotalAmount,
                po.Status,
                po.CreatedAt,
                po.UpdatedAt,
                po.ReceivedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    public record UpdatePoStatusRequest(PurchaseOrderStatus Status);

    [HttpPost("purchase-orders/{id:guid}/status")]
    [Authorize(Policy = "Vendor")]
    public async Task<ActionResult<ApiResponse<object>>> UpdatePurchaseOrderStatus(Guid id, [FromBody] UpdatePoStatusRequest request)
    {
        var profile = await RequireActiveVendorProfileAsync();
        var po = await _db.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == id && p.VendorProfileId == profile.Id)
            ?? throw new NotFoundException("Purchase order not found.");

        var allowed = po.Status switch
        {
            PurchaseOrderStatus.PendingVendorAcceptance => request.Status is PurchaseOrderStatus.Accepted or PurchaseOrderStatus.Cancelled,
            PurchaseOrderStatus.Accepted => request.Status is PurchaseOrderStatus.Processing or PurchaseOrderStatus.Cancelled,
            PurchaseOrderStatus.Processing => request.Status is PurchaseOrderStatus.Shipped or PurchaseOrderStatus.Cancelled,
            PurchaseOrderStatus.Shipped => request.Status is PurchaseOrderStatus.Delivered,
            _ => false
        };

        if (!allowed)
            throw new ConflictException($"Cannot transition purchase order from {po.Status} to {request.Status}.");

        po.Status = request.Status;
        po.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { po.Id, po.OrderNumber, po.Status }, "Purchase order status updated."));
    }
}

