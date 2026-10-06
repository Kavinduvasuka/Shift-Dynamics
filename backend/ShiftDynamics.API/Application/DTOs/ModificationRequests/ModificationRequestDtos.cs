using System.ComponentModel.DataAnnotations;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.DTOs.ModificationRequests;

public record CreateRequest(Guid VehicleId, [Required, StringLength(100)] string RequestType, [Required, StringLength(2000)] string Description);
public record ReviewRequest(ModificationRequestStatus Status, decimal? ProposedCost, string? AdvisorNotes);
