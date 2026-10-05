using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Interfaces;

public interface IContactService
{
    Task<ContactInquiry> CreateAsync(string name, string email, string? phone, string? type, string subject, string message);
    Task<IReadOnlyList<ContactInquiry>> ListAsync(InquiryStatus? status);
    Task<ContactInquiry?> UpdateStatusAsync(Guid id, InquiryStatus status);
}
