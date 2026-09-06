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
        await EnsureStaff("mechanic2@shiftdynamics.lk", "Kasun Technician", "+94110000005", SystemRole.Mechanic, "EMP-MEC-002");
        await EnsureStaff("mechanic3@shiftdynamics.lk", "Ruwan Technician", "+94110000006", SystemRole.Mechanic, "EMP-MEC-003");
        await EnsureStaff("store@shiftdynamics.lk", "Demo Storekeeper", "+94110000004", SystemRole.Storekeeper, "EMP-STK-001");

        // Demo workshop data
        if (!await _db.WorkOrders.AnyAsync())
        {
            var manager = await _db.Staff.FirstAsync(s => s.Role == SystemRole.Manager);

            var mechanics = await _db.Staff
                .Where(s => s.Role == SystemRole.Mechanic && s.Status == StaffStatus.Active)
                .OrderBy(s => s.EmployeeNumber)
                .ToListAsync();

            var brake = await _db.Services.FirstAsync(s => s.Name == "Brake Inspection");
            var oil = await _db.Services.FirstAsync(s => s.Name == "Oil Change");
            var full = await _db.Services.FirstAsync(s => s.Name == "Full Service");

            var bays = await _db.WorkshopBays.OrderBy(b => b.Name).ToListAsync();
            var bay1 = bays.First(b => b.Name == "Bay 1");
            var bay2 = bays.First(b => b.Name == "Bay 2");
            var bay3 = bays.First(b => b.Name == "Bay 3");
            var bay4 = bays.First(b => b.Name == "Bay 4");

            var c1 = new Customer { Id=Guid.NewGuid(), FirstName="Kasun", LastName="Perera", Phone="+94771234567", Email="kasun@example.com", Address="Colombo" };
            var c2 = new Customer { Id=Guid.NewGuid(), FirstName="Nadeesha", LastName="Fernando", Phone="+94772345678", Email="nadeesha@example.com", Address="Dehiwala" };
            var c3 = new Customer { Id=Guid.NewGuid(), FirstName="Ruwan", LastName="Silva", Phone="+94773456789", Email="ruwan@example.com", Address="Nugegoda" };

            var v1 = new Vehicle { Id=Guid.NewGuid(), CustomerId=c1.Id, RegistrationNumber="CAB-1234", Make="Toyota", Model="Corolla", Year=2022, Color="White" };
            var v2 = new Vehicle { Id=Guid.NewGuid(), CustomerId=c2.Id, RegistrationNumber="WP-5678", Make="Honda", Model="Vezel", Year=2021, Color="Black" };
            var v3 = new Vehicle { Id=Guid.NewGuid(), CustomerId=c3.Id, RegistrationNumber="CP-9012", Make="Nissan", Model="X-Trail", Year=2020, Color="Silver" };

            var w1 = new WorkOrder { Id=Guid.NewGuid(), CustomerId=c1.Id, VehicleId=v1.Id, ServiceId=brake.Id, WorkOrderNumber="JC-1054", Status=WorkOrderStatus.Open, Description="Brake inspection and service" };
            var w2 = new WorkOrder { Id=Guid.NewGuid(), CustomerId=c2.Id, VehicleId=v2.Id, ServiceId=oil.Id, WorkOrderNumber="JC-1053", Status=WorkOrderStatus.InProgress, AssignedStaffId=mechanics[0].Id, StartedAt=DateTime.UtcNow.AddHours(-1), Description="Routine service" };
            var w3 = new WorkOrder { Id=Guid.NewGuid(), CustomerId=c3.Id, VehicleId=v3.Id, ServiceId=full.Id, WorkOrderNumber="JC-1052", Status=WorkOrderStatus.Assigned, AssignedStaffId=mechanics[1].Id, Description="Full vehicle service" };
            var w4 = new WorkOrder { Id=Guid.NewGuid(), CustomerId=c1.Id, VehicleId=v1.Id, ServiceId=oil.Id, WorkOrderNumber="JC-1051", Status=WorkOrderStatus.Completed, CompletedAt=DateTime.UtcNow, Description="Oil change completed" };

            _db.Customers.AddRange(c1,c2,c3);
            _db.Vehicles.AddRange(v1,v2,v3);
            _db.WorkOrders.AddRange(w1,w2,w3,w4);

            bay1.Status = BayStatus.Occupied;
            bay2.Status = BayStatus.Occupied;
            bay3.Status = BayStatus.Maintenance;
            bay4.Status = BayStatus.Available;

            _db.JobAssignments.AddRange(
                new JobAssignment { Id=Guid.NewGuid(), WorkOrderId=w2.Id, MechanicStaffId=mechanics[0].Id, BayId=bay1.Id, AssignedByUserId=manager.UserId, AssignedAt=DateTime.UtcNow.AddHours(-1), IsActive=true },
                new JobAssignment { Id=Guid.NewGuid(), WorkOrderId=w3.Id, MechanicStaffId=mechanics[1].Id, BayId=bay2.Id, AssignedByUserId=manager.UserId, AssignedAt=DateTime.UtcNow.AddMinutes(-30), IsActive=true }
            );

            _db.Estimates.AddRange(
                new Estimate { Id=Guid.NewGuid(), WorkOrderId=w1.Id, EstimateNumber="EST-1001", LaborCost=50, PartsCost=90, Subtotal=140, TaxAmount=14, TotalAmount=154, Status=EstimateStatus.Sent },
                new Estimate { Id=Guid.NewGuid(), WorkOrderId=w3.Id, EstimateNumber="EST-1002", LaborCost=150, PartsCost=100, Subtotal=250, TaxAmount=25, TotalAmount=275, Status=EstimateStatus.Approved, CustomerApproved=true, ApprovedAt=DateTime.UtcNow.AddHours(-2) }
            );

            _db.Invoices.AddRange(
                new Invoice { Id=Guid.NewGuid(), WorkOrderId=w4.Id, InvoiceNumber="INV-1062", LaborCost=75, PartsCost=40, Subtotal=115, TaxAmount=11.50m, TotalAmount=126.50m, BalanceDue=126.50m, Status=InvoiceStatus.Draft },
                new Invoice { Id=Guid.NewGuid(), WorkOrderId=w2.Id, InvoiceNumber="INV-1061", LaborCost=75, PartsCost=40, Subtotal=115, TaxAmount=11.50m, TotalAmount=126.50m, AmountPaid=126.50m, BalanceDue=0, Status=InvoiceStatus.Paid }
            );
        }
        await _db.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Development seed data applied. Staff password: Test@1234"));
    }
}
