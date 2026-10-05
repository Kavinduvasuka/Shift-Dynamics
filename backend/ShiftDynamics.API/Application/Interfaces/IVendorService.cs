using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.Interfaces;

public interface IVendorService
{
    Task<VendorRegistration> RegisterAsync(string businessName, string contactPerson, string mobile, string email, string? address, string? specialization, string password);
    Task<IReadOnlyList<VendorRegistration>> ListRegistrationsAsync(VendorRegistrationStatus? status);
    Task<object?> ReviewRegistrationAsync(Guid id, bool approve, string? rejectionReason, Guid reviewerId);
    Task<VendorProfile?> GetProfileAsync(Guid userId);
    Task<IReadOnlyList<VendorProfile>> ListProfilesAsync(VendorApprovalStatus? status);
    Task<VendorProfile?> UpdateProfileStatusAsync(Guid id, VendorApprovalStatus status);
}

