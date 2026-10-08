using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/integration")]
[Authorize]
public class IntegrationController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;
    public IntegrationController(ShiftDynamicsDbContext db) => _db = db;

    [HttpGet("appointments")]
    [Authorize(Policy = "Staff")]
    public async Task<IActionResult> Appointments() => Ok(await _db.Appointments.AsNoTracking()
        .OrderByDescending(a => a.AppointmentDate).Select(a => new {
            a.Id, a.CustomerId, a.VehicleId, a.AppointmentDate, a.ServiceType, a.Notes, a.Status,
            CustomerName = a.Customer.FirstName + " " + a.Customer.LastName,
            VehicleRegistration = a.Vehicle.RegistrationNumber,
            HasJob = _db.WorkOrders.Any(w => w.AppointmentId == a.Id)
        }).ToListAsync());

    [HttpGet("emergencies")]
    public async Task<IActionResult> Emergencies()
    {
        var query = _db.EmergencyRequests.AsNoTracking().AsQueryable();
        if (User.IsInRole("Customer")) {
            var id = User.RequireCustomerId(); query = query.Where(r => r.CustomerId == id);
        } else if (!User.IsInRole("ServiceAdvisor") && !User.IsInRole("Manager") && !User.IsInRole("Admin")) {
            throw new ForbiddenException();
        }
        return Ok(await query.OrderByDescending(r => r.RequestedAt).Select(r => new {
            r.Id, r.Location, r.ProblemDescription, r.Status, r.RequestedAt, r.CompletedAt,
            CustomerName = r.Customer.FirstName + " " + r.Customer.LastName,
            Phone = r.Customer.Phone, VehicleRegistration = r.Vehicle != null ? r.Vehicle.RegistrationNumber : null
        }).ToListAsync());
    }

    public record EmergencyStatusRequest([Range(0,4)] int Status);
    [HttpPatch("emergencies/{id:guid}")]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<IActionResult> EmergencyStatus(Guid id, EmergencyStatusRequest request)
    {
        var item = await _db.EmergencyRequests.FindAsync(id) ?? throw new NotFoundException("Request not found.");
        if (item.Status is EmergencyRequestStatus.Completed or EmergencyRequestStatus.Cancelled)
            throw new ConflictException("This request is closed.");
        item.Status = (EmergencyRequestStatus)request.Status;
        if (item.Status == EmergencyRequestStatus.Accepted) item.AcceptedAt = DateTime.UtcNow;
        if (item.Status == EmergencyRequestStatus.Completed) item.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { item.Id, item.Status });
    }

    [HttpGet("parts")]
    [Authorize(Policy = "Staff")]
    public async Task<IActionResult> Parts() => Ok(await _db.Parts.AsNoTracking().Where(p => p.IsActive)
        .OrderBy(p => p.Name).Select(p => new { p.Id, p.PartNumber, p.Name, p.Category }).ToListAsync());

    [HttpGet("quotes")]
    public async Task<IActionResult> Quotes()
    {
        var query = _db.VendorQuotes.AsNoTracking().AsQueryable();
        if (User.IsInRole("Vendor")) { var id = User.RequireUserId(); query = query.Where(q => q.VendorProfile.UserId == id); }
        else if (!User.IsInRole("Manager") && !User.IsInRole("Admin")) throw new ForbiddenException();
        return Ok(await query.OrderByDescending(q => q.SubmittedAt).Select(q => new {
            q.Id, q.QuoteRequestId, q.UnitPrice, q.AvailableQuantity, q.DeliveryDays, q.Status, q.Notes,
            RequestNumber = q.QuoteRequest.RequestNumber, PartDescription = q.QuoteRequest.PartDescription,
            BusinessName = q.VendorProfile.BusinessName
        }).ToListAsync());
    }

    [HttpGet("purchase-orders")]
    public async Task<IActionResult> Orders()
    {
        var query = _db.PurchaseOrders.AsNoTracking().AsQueryable();
        if (User.IsInRole("Vendor")) { var id = User.RequireUserId(); query = query.Where(o => o.VendorQuote.VendorProfile.UserId == id); }
        else if (!User.IsInRole("Manager") && !User.IsInRole("Storekeeper") && !User.IsInRole("Admin")) throw new ForbiddenException();
        return Ok(await query.OrderByDescending(o => o.ApprovedAt).Select(o => new {
            o.Id, o.PurchaseOrderNumber, o.Quantity, o.ReceivedQuantity, o.UnitPrice, o.Status,
            o.ExpectedDeliveryAt, o.ReceivedAt, PartDescription = o.QuoteRequest.PartDescription,
            BusinessName = o.VendorQuote.VendorProfile.BusinessName
        }).ToListAsync());
    }

    [HttpGet("staff")]
    [Authorize(Policy = "Manager")]
    public async Task<IActionResult> StaffList() => Ok(await _db.Staff.AsNoTracking().OrderBy(s => s.User.FullName)
        .Select(s => new { s.Id, s.EmployeeNumber, s.User.FullName, s.User.Email, s.User.Phone, s.Role, s.Status }).ToListAsync());

    public class StaffRequest {
        [Required, StringLength(150, MinimumLength=2)] public string FullName {get;set;} = "";
        [Required, EmailAddress, StringLength(191)] public string Email {get;set;} = "";
        [Required, StringLength(30, MinimumLength=7)] public string Phone {get;set;} = "";
        [Required, StringLength(100, MinimumLength=8)] public string Password {get;set;} = "";
        [Required, StringLength(50)] public string EmployeeNumber {get;set;} = "";
        [Range(1,4)] public int Role {get;set;}
    }
    [HttpPost("staff")]
    [Authorize(Policy = "Manager")]
    public async Task<IActionResult> CreateStaff(StaffRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant(); var phone = request.Phone.Trim();
        if (await _db.Users.AnyAsync(u => u.Email == email || u.Phone == phone)) throw new ConflictException("Email or phone already registered.");
        if (await _db.Staff.AnyAsync(s => s.EmployeeNumber == request.EmployeeNumber.Trim())) throw new ConflictException("Employee number already exists.");
        var user = new User { Id=Guid.NewGuid(), FullName=request.FullName.Trim(), Email=email, Phone=phone,
            PasswordHash=BCrypt.Net.BCrypt.HashPassword(request.Password), Role=(SystemRole)request.Role, Status=AccountStatus.Active };
        var staff = new Staff { Id=Guid.NewGuid(), UserId=user.Id, User=user, EmployeeNumber=request.EmployeeNumber.Trim(),
            Role=user.Role, Status=StaffStatus.Active };
        _db.Users.Add(user); _db.Staff.Add(staff); await _db.SaveChangesAsync();
        return Ok(new { staff.Id, user.FullName, user.Email, user.Role });
    }

    // An empty development database needs its first manager before protected staff creation is available.
    [HttpPost("bootstrap-manager")]
    [AllowAnonymous]
    public async Task<IActionResult> Bootstrap(StaffRequest request, [FromServices] IHostEnvironment env)
    {
        if (!env.IsDevelopment()) throw new ForbiddenException("Development setup only.");
        var remote = HttpContext.Connection.RemoteIpAddress;
        if (remote == null || !System.Net.IPAddress.IsLoopback(remote)) throw new ForbiddenException("Run initial manager setup on the backend PC.");
        if (request.Role != (int)SystemRole.Manager) throw new ShiftDynamics.API.Common.ValidationException("The first account must be a manager.");
        return await _db.Database.CreateExecutionStrategy().ExecuteAsync<IActionResult>(async () =>
        {
        // Serialize concurrent setup requests; never allow a second manager bootstrap.
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        if (await _db.Users.AnyAsync(u => u.Role == SystemRole.Manager || u.Role == SystemRole.Admin))
            throw new ConflictException("A manager already exists. Use the staff login.");
        var result = await CreateStaff(request); await tx.CommitAsync(); return result;
        });
    }

    public class IntakeRequest {
        [Required, StringLength(150,MinimumLength=2)] public string FullName {get;set;} = "";
        [Required, EmailAddress, StringLength(191)] public string Email {get;set;} = "";
        [Required, StringLength(30,MinimumLength=7)] public string Phone {get;set;} = "";
        [Required, StringLength(100,MinimumLength=8)] public string Password {get;set;} = "";
        [StringLength(500)] public string Address {get;set;} = "";
    }
    [HttpPost("customers")]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<IActionResult> Intake(IntakeRequest request,
        [FromServices] ShiftDynamics.API.Application.Services.IAuthService auth)
    {
        var result = await auth.RegisterCustomerAsync(new ShiftDynamics.API.Application.DTOs.Auth.RegisterRequest {
            FullName=request.FullName, Email=request.Email, Phone=request.Phone, Password=request.Password, Address=request.Address
        });
        return Ok(new { result.CustomerId, result.FullName, result.Email });
    }

    public class StaffVehicleRequest {
        public Guid CustomerId {get;set;}
        [Required,StringLength(30)] public string RegistrationNumber {get;set;} = "";
        [Required,StringLength(100)] public string Make {get;set;} = "";
        [Required,StringLength(100)] public string Model {get;set;} = "";
        [Range(1900,2100)] public int Year {get;set;}
        [StringLength(50)] public string? VIN {get;set;}
    }
    [HttpPost("vehicles")]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<IActionResult> StaffVehicle(StaffVehicleRequest request,
        [FromServices] ShiftDynamics.API.Application.Interfaces.IVehicleService vehicles)
    {
        if (!await _db.Customers.AnyAsync(c => c.Id == request.CustomerId)) throw new NotFoundException("Customer not found.");
        return Ok(await vehicles.CreateAsync(request.CustomerId, new ShiftDynamics.API.Application.DTOs.Vehicles.CreateVehicleRequest {
            CustomerId=request.CustomerId, RegistrationNumber=request.RegistrationNumber, Make=request.Make, Model=request.Model, Year=request.Year, VIN=request.VIN
        }));
    }
}
