using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/modification-jobs")]
[Authorize(Roles = "ServiceAdvisor,Manager,Admin")]
public class ModificationJobsController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;
    private const string Prefix = "WO-MOD-";

    public ModificationJobsController(ShiftDynamicsDbContext db)
        => _db = db;

    public record CreateModificationJobRequest(Guid ServiceId);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List()
    {
        var jobs = await _db.WorkOrders.AsNoTracking()
            .Where(w => w.WorkOrderNumber.StartsWith(Prefix))
            .Select(w => new
            {
                w.Id,
                w.WorkOrderNumber,
                w.Status
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(jobs));
    }

    [HttpPost("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        Guid id,
        [FromBody] CreateModificationJobRequest input)
    {
        var modification = await _db.ModificationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("Modification request not found.");

        // A stable, unique job number links this job to its modification.
        var number = Prefix + id.ToString("N");

        var existing = await _db.WorkOrders.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WorkOrderNumber == number);

        if (existing is not null)
            return JobResult(existing, true);

        if (modification.Status != ModificationRequestStatus.Approved)
            throw new ConflictException(
                "The customer must approve the modification quotation first.");

        if (!modification.ProposedCost.HasValue ||
            modification.ProposedCost.Value < 0)
            throw new ValidationException(
                "An accepted price is required before creating the job.");

        if (!await _db.Vehicles.AnyAsync(v =>
            v.Id == modification.VehicleId &&
            v.CustomerId == modification.CustomerId))
            throw new ValidationException(
                "The vehicle does not belong to this customer.");

        if (!await _db.Services.AnyAsync(s =>
            s.Id == input.ServiceId && s.IsActive))
            throw new ValidationException("Select an active service package.");

        var price = modification.ProposedCost.Value
            .ToString("N2", CultureInfo.InvariantCulture);

        var description =
            $"Approved modification: {modification.RequestType}\n" +
            $"Accepted price: LKR {price}\n" +
            $"Modification reference: {modification.Id}\n" +
            $"Requested changes: {modification.Description}\n" +
            $"Advisor notes: {modification.AdvisorNotes}";

        // The existing job description column allows 2000 characters.
        // The full request remains available in Modifications.
        if (description.Length > 2000)
        {
            const string suffix =
                "\nSee the approved modification for full details.";
            description = description[..(2000 - suffix.Length)] + suffix;
        }

        var now = DateTime.UtcNow;

        var job = new WorkOrder
        {
            Id = Guid.NewGuid(),
            WorkOrderNumber = number,
            CustomerId = modification.CustomerId,
            VehicleId = modification.VehicleId,
            ServiceId = input.ServiceId,
            AppointmentId = null,
            Description = description,
            Status = WorkOrderStatus.Open,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.WorkOrders.Add(job);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // The existing unique job-number index also prevents
            // duplicate jobs from simultaneous submissions.
            _db.ChangeTracker.Clear();

            existing = await _db.WorkOrders.AsNoTracking()
                .FirstOrDefaultAsync(w => w.WorkOrderNumber == number);

            if (existing is not null)
                return JobResult(existing, true);

            throw;
        }

        return JobResult(job, false);
    }

    private ActionResult<ApiResponse<object>> JobResult(
        WorkOrder job, bool alreadyExists)
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            job.Id,
            job.WorkOrderNumber,
            job.Status,
            AlreadyExists = alreadyExists
        }, alreadyExists
            ? "A job already exists for this modification."
            : "Job created. The manager can now assign a mechanic."));
    }
}