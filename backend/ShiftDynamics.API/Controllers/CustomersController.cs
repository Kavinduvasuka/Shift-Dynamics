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
    private readonly ShiftDynamicsDbContext _db;

    public CustomersController(ShiftDynamicsDbContext db) => _db = db;

    private static CustomerResponse Map(Customer c) => new()
    {
        Id = c.Id,
        FirstName = c.FirstName,
        LastName = c.LastName,
        Phone = c.Phone,
        Email = c.Email,
        Address = c.Address,
        CreatedAt = c.CreatedAt,
        VehicleCount = c.Vehicles.Count
    };

    [HttpGet]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetCustomers()
    {
        var customers = await _db.Customers
            .AsNoTracking()
            .Include(c => c.Vehicles)
            .ToListAsync();

        return Ok(customers.Select(Map));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomer(Guid id)
    {
        var role = User.GetRole();

        if (role == SystemRole.Customer.ToString())
        {
            if (id != User.RequireCustomerId())
                throw new ForbiddenException();
        }
        else if (role is not ("ServiceAdvisor" or "Manager" or "Admin"))
        {
            throw new ForbiddenException();
        }

        var customer = await _db.Customers
            .AsNoTracking()
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == id);

        return customer is null
            ? NotFound()
            : Ok(Map(customer));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        UpdateCustomerRequest request)
    {
        var role = User.GetRole();

        if (role == SystemRole.Customer.ToString())
        {
            if (id != User.RequireCustomerId())
                throw new ForbiddenException();
        }
        else if (role is not ("ServiceAdvisor" or "Manager" or "Admin"))
        {
            throw new ForbiddenException();
        }

        var customer = await _db.Customers.FindAsync(id);

        if (customer is null)
            return NotFound();

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Phone = request.Phone.Trim();
        customer.Email = request.Email.Trim();
        customer.Address = request.Address.Trim();

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
