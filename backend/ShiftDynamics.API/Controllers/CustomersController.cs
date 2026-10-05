using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.DTOs.Customers;
using ShiftDynamics.API.Interfaces;

namespace ShiftDynamics.API.Controllers;
[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
 private readonly ICustomerService _customers; public CustomersController(ICustomerService customers)=>_customers=customers;
 [HttpGet][Authorize(Policy="ServiceAdvisor")] public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetCustomers()=>Ok(await _customers.GetAllAsync());
 [HttpGet("{id:guid}")] public async Task<ActionResult<CustomerResponse>> GetCustomer(Guid id){EnsureCustomerOrAdvisor(id);var c=await _customers.GetByIdAsync(id);return c is null?NotFound():Ok(c);}
 [HttpPut("{id:guid}")] public async Task<IActionResult> UpdateCustomer(Guid id,UpdateCustomerRequest request){EnsureCustomerOrAdvisor(id);return await _customers.UpdateAsync(id,request)?NoContent():NotFound();}
 private void EnsureCustomerOrAdvisor(Guid customerId)
 {
     if (User.IsInRole(SystemRole.Customer.ToString())) { if (customerId != User.RequireCustomerId()) throw new ForbiddenException(); return; }
     if (!User.IsInRole(SystemRole.ServiceAdvisor.ToString()) && !User.IsInRole(SystemRole.Manager.ToString()) && !User.IsInRole(SystemRole.Admin.ToString())) throw new ForbiddenException();
 }
}
