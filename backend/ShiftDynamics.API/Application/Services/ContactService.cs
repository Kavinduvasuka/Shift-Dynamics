using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class ContactService : IContactService
{
    private readonly ShiftDynamicsDbContext _db;
    public ContactService(ShiftDynamicsDbContext db) => _db = db;
    public async Task<ContactInquiry> CreateAsync(string name, string email, string? phone, string? type, string subject, string message)
    {
        var inquiry = new ContactInquiry { Id = Guid.NewGuid(), Name = name.Trim(), Email = email.Trim().ToLowerInvariant(), Phone = phone?.Trim(), Type = type?.Trim(), Subject = subject.Trim(), Message = message.Trim(), Status = InquiryStatus.New, CreatedAt = DateTime.UtcNow };
        _db.ContactInquiries.Add(inquiry); await _db.SaveChangesAsync(); return inquiry;
    }
    public async Task<IReadOnlyList<ContactInquiry>> ListAsync(InquiryStatus? status)
    {
        var query = _db.ContactInquiries.AsNoTracking().AsQueryable(); if (status.HasValue) query = query.Where(i => i.Status == status.Value);
        return await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
    }
    public async Task<ContactInquiry?> UpdateStatusAsync(Guid id, InquiryStatus status)
    {
        var inquiry = await _db.ContactInquiries.FirstOrDefaultAsync(i => i.Id == id); if (inquiry is null) return null;
        inquiry.Status = status; if (status == InquiryStatus.Read && inquiry.ReadAt is null) inquiry.ReadAt = DateTime.UtcNow; if (status == InquiryStatus.Resolved) inquiry.ResolvedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(); return inquiry;
    }
}

