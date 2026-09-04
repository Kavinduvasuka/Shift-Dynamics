using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.DTOs.Customers;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;
[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
 private readonly ShiftDynamicsDbContext _db; public CustomersController(ShiftDynamicsDbContext db)=>_db=db;
 private static CustomerResponse Map(Customer c)=>new(){Id=c.Id,FirstName=c.FirstName,LastName=c.LastName,Phone=c.Phone,Email=c.Email,Address=c.Address,CreatedAt=c.CreatedAt,VehicleCount=c.Vehicles.Count};
 [HttpGet][Authorize(Policy="Manager")] public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetCustomers()=>Ok((await _db.Customers.AsNoTracking().Include(c=>c.Vehicles).ToListAsync()).Select(Map));
 [HttpGet("{id:guid}")] public async Task<ActionResult<CustomerResponse>> GetCustomer(Guid id){if(User.IsInRole(SystemRole.Customer.ToString())&&id!=User.RequireCustomerId())throw new ForbiddenException();var c=await _db.Customers.AsNoTracking().Include(c=>c.Vehicles).FirstOrDefaultAsync(c=>c.Id==id);return c is null?NotFound():Ok(Map(c));}
 [HttpPut("{id:guid}")] public async Task<IActionResult> UpdateCustomer(Guid id,UpdateCustomerRequest request){if(User.IsInRole(SystemRole.Customer.ToString())&&id!=User.RequireCustomerId())throw new ForbiddenException();var c=await _db.Customers.FindAsync(id);if(c is null)return NotFound();c.FirstName=request.FirstName.Trim();c.LastName=request.LastName.Trim();c.Phone=request.Phone.Trim();c.Email=request.Email.Trim();c.Address=request.Address.Trim();await _db.SaveChangesAsync();return NoContent();}
}