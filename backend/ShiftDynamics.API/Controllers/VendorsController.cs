using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;
using ShiftDynamics.API.Application.DTOs.Vendors;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/vendors")]
public class VendorsController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;
    private readonly IVendorService _vendors;

    public VendorsController(ShiftDynamicsDbContext db, IVendorService vendors) { _db = db; _vendors = vendors; }

    [HttpPost("registrations")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] VendorRegisterRequest request)
    {
        var reg = await _vendors.RegisterAsync(request.BusinessName, request.ContactPerson, request.Mobile, request.Email, request.Address, request.Specialization, request.Password);

        return Ok(ApiResponse<object>.Ok(new { reg.Id, reg.Status }, "Vendor registration submitted for review."));
    }

    [HttpGet("registrations")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> ListRegistrations([FromQuery] VendorRegistrationStatus? status)
    {
        var items = (await _vendors.ListRegistrationsAsync(status))
            .Select(v => new
            {
                v.Id, v.BusinessName, v.ContactPerson, v.Mobile, v.Email,
                v.Address, v.Specialization, v.Status, v.SubmittedAt, v.ReviewedAt, v.RejectionReason
            }).ToList();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpPost("registrations/{id:guid}/review")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> Review(Guid id, [FromBody] ReviewVendorRequest request)
    {
        var result = await _vendors.ReviewRegistrationAsync(id, request.Approve, request.RejectionReason, User.RequireUserId()) ?? throw new NotFoundException("Registration not found.");
        return Ok(ApiResponse<object>.Ok(result, request.Approve ? "Vendor approved and account activated." : "Vendor registration rejected."));
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

        var profile = await _vendors.GetProfileAsync(userId)
            ?? throw new NotFoundException("Vendor profile not found.");

        return Ok(ApiResponse<object>.Ok(profile));
    }

    /// <summary>Manager: list vendor profiles.</summary>
    [HttpGet("profiles")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> ListProfiles([FromQuery] VendorApprovalStatus? status)
    {
        var items = (await _vendors.ListProfilesAsync(status))
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
            }).ToList();

        return Ok(ApiResponse<object>.Ok(items));
    }

    /// <summary>Manager: suspend / reactivate a vendor profile.</summary>
    [HttpPatch("profiles/{id:guid}/status")]
    [Authorize(Policy = "Manager")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProfileStatus(Guid id, [FromBody] UpdateVendorStatusRequest request)
    {
        var profile = await _vendors.UpdateProfileStatusAsync(id, request.Status)
            ?? throw new NotFoundException("Vendor profile not found.");
        return Ok(ApiResponse<object>.Ok(profile, "Vendor status updated."));
    }
}

