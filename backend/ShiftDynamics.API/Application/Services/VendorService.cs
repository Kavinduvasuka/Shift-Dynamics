using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class VendorService : IVendorService
{
    private readonly ShiftDynamicsDbContext _db;
    public VendorService(ShiftDynamicsDbContext db) => _db = db;
    public async Task<VendorRegistration> RegisterAsync(string businessName, string contactPerson, string mobile, string email, string? address, string? specialization, string password)
    { var normalizedEmail = email.Trim().ToLowerInvariant(); if (await _db.VendorRegistrations.AnyAsync(v => v.Email == normalizedEmail && v.Status == VendorRegistrationStatus.Pending) || await _db.Users.AnyAsync(u => u.Email == normalizedEmail)) throw new ConflictException("An account or pending registration already exists for this email."); var item = new VendorRegistration { Id = Guid.NewGuid(), BusinessName = businessName.Trim(), ContactPerson = contactPerson.Trim(), Mobile = mobile.Trim(), Email = normalizedEmail, Address = address?.Trim(), Specialization = specialization?.Trim(), PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), Status = VendorRegistrationStatus.Pending, SubmittedAt = DateTime.UtcNow }; _db.VendorRegistrations.Add(item); await _db.SaveChangesAsync(); return item; }
    public async Task<IReadOnlyList<VendorRegistration>> ListRegistrationsAsync(VendorRegistrationStatus? status) { var query = _db.VendorRegistrations.AsNoTracking().AsQueryable(); if (status.HasValue) query = query.Where(v => v.Status == status.Value); return await query.OrderByDescending(v => v.SubmittedAt).ToListAsync(); }
    public async Task<object?> ReviewRegistrationAsync(Guid id, bool approve, string? rejectionReason, Guid reviewerId)
    { await using var transaction = await _db.Database.BeginTransactionAsync(); var reg = await _db.VendorRegistrations.FirstOrDefaultAsync(v => v.Id == id); if (reg is null) return null; if (reg.Status != VendorRegistrationStatus.Pending) throw new ConflictException("Registration is not pending."); reg.ReviewedByUserId = reviewerId; reg.ReviewedAt = DateTime.UtcNow; if (!approve) { reg.Status = VendorRegistrationStatus.Rejected; reg.RejectionReason = rejectionReason?.Trim(); await _db.SaveChangesAsync(); return reg; } var user = new User { Id = Guid.NewGuid(), FullName = reg.ContactPerson, Email = reg.Email, Phone = reg.Mobile, PasswordHash = reg.PasswordHash, Role = SystemRole.Vendor, Status = AccountStatus.Active, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }; var profile = new VendorProfile { Id = Guid.NewGuid(), UserId = user.Id, RegistrationId = reg.Id, BusinessName = reg.BusinessName, ContactPerson = reg.ContactPerson, Mobile = reg.Mobile, Email = reg.Email, Address = reg.Address, Specialization = reg.Specialization, ApprovalStatus = VendorApprovalStatus.Active, ApprovedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }; reg.Status = VendorRegistrationStatus.Approved; reg.CreatedUserId = user.Id; _db.Users.Add(user); _db.VendorProfiles.Add(profile); await _db.SaveChangesAsync(); await transaction.CommitAsync(); return new { reg.Id, UserId = user.Id, VendorProfileId = profile.Id }; }
    public async Task<VendorProfile?> GetProfileAsync(Guid userId) => await _db.VendorProfiles.AsNoTracking().FirstOrDefaultAsync(v => v.UserId == userId);
    public async Task<IReadOnlyList<VendorProfile>> ListProfilesAsync(VendorApprovalStatus? status) { var query = _db.VendorProfiles.AsNoTracking().AsQueryable(); if (status.HasValue) query = query.Where(v => v.ApprovalStatus == status.Value); return await query.OrderBy(v => v.BusinessName).ToListAsync(); }
    public async Task<VendorProfile?> UpdateProfileStatusAsync(Guid id, VendorApprovalStatus status) { var profile = await _db.VendorProfiles.Include(v => v.User).FirstOrDefaultAsync(v => v.Id == id); if (profile is null) return null; profile.ApprovalStatus = status; profile.UpdatedAt = DateTime.UtcNow; profile.User.Status = status == VendorApprovalStatus.Active ? AccountStatus.Active : AccountStatus.Inactive; profile.User.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return profile; }
}

