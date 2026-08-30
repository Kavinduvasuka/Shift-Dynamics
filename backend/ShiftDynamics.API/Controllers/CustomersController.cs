using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.DTOs.Customers;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _dbContext;

    public CustomersController(ShiftDynamicsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetCustomers()
    {
        var customers = await _dbContext.Customers
            .AsNoTracking()
            .Include(c => c.Vehicles)
            .Select(c => new CustomerResponse
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                CreatedAt = c.CreatedAt,
                VehicleCount = c.Vehicles.Count
            })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomer(Guid id)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .Include(c => c.Vehicles)
            .Where(c => c.Id == id)
            .Select(c => new CustomerResponse
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                CreatedAt = c.CreatedAt,
                VehicleCount = c.Vehicles.Count
            })
            .FirstOrDefaultAsync();

        if (customer is null)
            return NotFound();

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer(
        CreateCustomerRequest request)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email.Trim(),
            Address = request.Address.Trim()
        };

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        var response = new CustomerResponse
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt,
            VehicleCount = 0
        };

        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = customer.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        UpdateCustomerRequest request)
    {
        var customer = await _dbContext.Customers.FindAsync(id);

        if (customer is null)
            return NotFound();

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Phone = request.Phone.Trim();
        customer.Email = request.Email.Trim();
        customer.Address = request.Address.Trim();

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        var customer = await _dbContext.Customers
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null)
            return NotFound();

        if (customer.Vehicles.Count > 0)
            return Conflict(new
            {
                message = "Customer cannot be deleted while vehicles are linked to the customer."
            });

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
