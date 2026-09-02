using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

/// <summary>
/// Development-only seed endpoint. Disable or protect in production.
/// </summary>
[ApiController]
[Route("api/seed")]
public class SeedController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;
    private readonly IHostEnvironment _env;

    public SeedController(ShiftDynamicsDbContext db, IHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> Seed()
    {
        if (!_env.IsDevelopment())
            throw new ForbiddenException("Seeding is only allowed in Development.");

        // Services catalog
        if (!await _db.Services.AnyAsync())
        {
            _db.Services.AddRange(
                new Service { Id = Guid.NewGuid(), Name = "Oil Change", Description = "Full synthetic oil change", BasePrice = 75, EstimatedDurationMinutes = 45, IsActive = true },
                new Service { Id = Guid.NewGuid(), Name = "Brake Inspection", Description = "Complete brake system check", BasePrice = 50, EstimatedDurationMinutes = 30, IsActive = true },
                new Service { Id = Guid.NewGuid(), Name = "Full Service", Description = "Comprehensive vehicle service", BasePrice = 250, EstimatedDurationMinutes = 180, IsActive = true },
                new Service { Id = Guid.NewGuid(), Name = "Wheel Alignment", Description = "4-wheel alignment", BasePrice = 90, EstimatedDurationMinutes = 60, IsActive = true }
            );
        }

        // Workshop bays
        if (!await _db.WorkshopBays.AnyAsync())
        {
            _db.WorkshopBays.AddRange(
                new WorkshopBay { Id = Guid.NewGuid(), Name = "Bay 1", Status = BayStatus.Available },
                new WorkshopBay { Id = Guid.NewGuid(), Name = "Bay 2", Status = BayStatus.Available },
                new WorkshopBay { Id = Guid.NewGuid(), Name = "Bay 3", Status = BayStatus.Available },
                new WorkshopBay { Id = Guid.NewGuid(), Name = "Bay 4", Status = BayStatus.Maintenance }
            );
        }

        // Emergency providers (Colombo-ish sample coords)
        if (!await _db.EmergencyServiceProviders.AnyAsync())
        {
            _db.EmergencyServiceProviders.AddRange(
                new EmergencyServiceProvider { Id = Guid.NewGuid(), Name = "Shift Dynamics Main Garage", Category = "Garage", Phone = "+94112223344", Latitude = 6.9271m, Longitude = 79.8612m, OpeningHours = "08:00-18:00", IsActive = true },
                new EmergencyServiceProvider { Id = Guid.NewGuid(), Name = "City Towing 24/7", Category = "Towing", Phone = "+94771234567", Latitude = 6.9344m, Longitude = 79.8428m, OpeningHours = "24/7", IsActive = true },
                new EmergencyServiceProvider { Id = Guid.NewGuid(), Name = "Mobile Mech Express", Category = "Mobile", Phone = "+94779876543", Latitude = 6.9022m, Longitude = 79.8610m, OpeningHours = "07:00-22:00", IsActive = true }
            );
        }

        // Sample parts
        if (!await _db.Parts.AnyAsync())
        {
            var oilFilter = new Part { Id = Guid.NewGuid(), PartNumber = "OF-001", Name = "Oil Filter", Category = "Filters", IsActive = true };
            var brakePads = new Part { Id = Guid.NewGuid(), PartNumber = "BP-001", Name = "Brake Pads (Front)", Category = "Brakes", IsActive = true };
            _db.Parts.AddRange(oilFilter, brakePads);
            _db.InventoryItems.AddRange(
                new InventoryItem { Id = Guid.NewGuid(), PartId = oilFilter.Id, OnHandQty = 40, ReorderLevel = 10, UnitCost = 12.50m, Location = "A-1" },
                new InventoryItem { Id = Guid.NewGuid(), PartId = brakePads.Id, OnHandQty = 8, ReorderLevel = 10, UnitCost = 45.00m, Location = "B-2" }
            );
        }

        // Demo staff users (password: Test@1234)
        async Task EnsureStaff(string email, string name, string phone, SystemRole role, string empNo)
        {
            if (await _db.Users.AnyAsync(u => u.Email == email)) return;

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = name,
                Email = email,
                Phone = phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234"),
                Role = role,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var staff = new Staff
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                EmployeeNumber = empNo,
                Role = role,
                Status = StaffStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            _db.Staff.Add(staff);
        }

        await EnsureStaff("manager@shiftdynamics.lk", "Demo Manager", "+94110000001", SystemRole.Manager, "EMP-MGR-001");
        await EnsureStaff("advisor@shiftdynamics.lk", "Demo Advisor", "+94110000002", SystemRole.ServiceAdvisor, "EMP-ADV-001");
        await EnsureStaff("mechanic@shiftdynamics.lk", "Demo Mechanic", "+94110000003", SystemRole.Mechanic, "EMP-MEC-001");
        await EnsureStaff("store@shiftdynamics.lk", "Demo Storekeeper", "+94110000004", SystemRole.Storekeeper, "EMP-STK-001");

        await _db.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Development seed data applied. Staff password: Test@1234"));
    }
}
