using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;
using ValidationException = ShiftDynamics.API.Common.ValidationException;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/wireframe")]
[Authorize]
public class WireframeController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;
    public WireframeController(ShiftDynamicsDbContext db) => _db = db;

    private async Task<WorkOrder> AccessibleJob(Guid id)
    {
        if (!(User.IsInRole("Customer") || User.IsInRole("Mechanic") || User.IsInRole("ServiceAdvisor") || User.IsInRole("Manager") || User.IsInRole("Admin")))
            throw new ForbiddenException();
        var w = await _db.WorkOrders.Include(w=>w.Vehicle).Include(w=>w.Customer).Include(w=>w.Service).FirstOrDefaultAsync(w=>w.Id==id)
            ?? throw new NotFoundException("Job card not found.");
        if (User.IsInRole("Customer") && w.CustomerId != User.RequireCustomerId()) throw new ForbiddenException();
        if (User.IsInRole("Mechanic"))
        {
            var uid = User.RequireUserId();
            if (!await _db.JobAssignments.AnyAsync(a=>a.WorkOrderId==id && a.Mechanic.UserId==uid)) throw new ForbiddenException();
        }
        return w;
    }

    [HttpGet("jobs/{id:guid}")]
    public async Task<IActionResult> Job(Guid id)
    {
        var w=await AccessibleJob(id);
        return Ok(new {
            w.Id, w.WorkOrderNumber, w.Status, w.Description, w.TechnicianNotes, w.StartedAt, w.CompletedAt,
            Vehicle=new{w.Vehicle.RegistrationNumber,w.Vehicle.Make,w.Vehicle.Model},
            CustomerName=w.Customer.FirstName+" "+w.Customer.LastName, ServiceName=w.Service.Name,
            Inspection=ReadBlock(w.Description,"INSPECTION"), Handover=ReadBlock(w.Description,"HANDOVER"),
            Diagnostics=await _db.DiagnosticFindings.AsNoTracking().Where(x=>x.WorkOrderId==id).OrderByDescending(x=>x.CreatedAt).Select(x=>new{x.Id,x.Finding,x.Severity,x.CreatedAt}).ToListAsync(),
            Repairs=await _db.RepairActions.AsNoTracking().Where(x=>x.WorkOrderId==id).OrderByDescending(x=>x.CreatedAt).Select(x=>new{x.Id,x.Action,x.Notes,x.CreatedAt}).ToListAsync(),
            Recommendations=await _db.MechanicRecommendations.AsNoTracking().Where(x=>x.WorkOrderId==id).OrderByDescending(x=>x.CreatedAt).Select(x=>new{x.Id,x.Recommendation,x.Priority,x.CreatedAt}).ToListAsync()
        });
    }

    // Persist checklists in marked blocks within the existing job description.
    // The EF model and SQL schema remain unchanged; records survive browser refresh.
    private static string? ReadBlock(string? text,string name)
    {
        var match=Regex.Match(text??"", $@"\[WF:{name}\](.*?)\[/WF:{name}\]",RegexOptions.Singleline);
        return match.Success?match.Groups[1].Value:null;
    }
    private static void WriteBlock(WorkOrder w,string name,object value)
    {
        var text=Regex.Replace(w.Description??"",$@"\s*\[WF:{name}\].*?\[/WF:{name}\]","",RegexOptions.Singleline);
        var next=text+"\n[WF:"+name+"]"+JsonSerializer.Serialize(value)+"[/WF:"+name+"]";
        if(next.Length>2000) throw new ValidationException("The job description and checklist exceed 2,000 characters. Shorten the checklist notes.");
        w.Description=next.Trim();w.UpdatedAt=DateTime.UtcNow;
    }
    public record InspectionRequest(bool Body,bool Tires,bool Lights,bool Brakes,bool Fluids,bool Interior,[StringLength(400)]string? Notes);
    [HttpPut("jobs/{id:guid}/inspection")]
    [Authorize(Roles="ServiceAdvisor,Manager,Admin")]
    public async Task<IActionResult> Inspection(Guid id,InspectionRequest input)
    {
        var w=await AccessibleJob(id);
        if(w.Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled) throw new ConflictException("A closed job cannot be inspected.");
        WriteBlock(w,"INSPECTION",new{input.Body,input.Tires,input.Lights,input.Brakes,input.Fluids,input.Interior,input.Notes,RecordedAt=DateTime.UtcNow});
        await _db.SaveChangesAsync(); return Ok(new{message="Inspection saved."});
    }
    public record HandoverRequest(bool WorkComplete,bool QualityChecked,bool CustomerInformed,bool PaymentConfirmed,bool DocumentsGiven,[StringLength(300)]string? Notes);
    [HttpPut("jobs/{id:guid}/handover")]
    [Authorize(Roles="ServiceAdvisor,Manager,Admin")]
    public async Task<IActionResult> Handover(Guid id,HandoverRequest input)
    {
        var w=await AccessibleJob(id);
        if(w.Status!=WorkOrderStatus.Completed) throw new ConflictException("Complete the workshop job before handover.");
        if(!(input.WorkComplete&&input.QualityChecked&&input.CustomerInformed&&input.PaymentConfirmed&&input.DocumentsGiven)) throw new ValidationException("Complete every handover check.");
        var bills=await _db.Invoices.AsNoTracking().Where(i=>i.WorkOrderId==id && i.Status!=InvoiceStatus.Cancelled).ToListAsync();
        if(bills.Count==0 || bills.Any(i=>i.Status!=InvoiceStatus.Paid || i.BalanceDue>0)) throw new ConflictException("All invoices must be issued and fully paid before final handover.");
        WriteBlock(w,"HANDOVER",new{input.WorkComplete,input.QualityChecked,input.CustomerInformed,input.PaymentConfirmed,input.DocumentsGiven,input.Notes,RecordedAt=DateTime.UtcNow});
        await _db.SaveChangesAsync();return Ok(new{message="Vehicle handover recorded."});
    }

    [HttpGet("mechanic/sessions")]
    [Authorize(Roles="Mechanic")]
    public async Task<IActionResult> Sessions()
    {
        var uid=User.RequireUserId();
        return Ok(await _db.LaborSessions.AsNoTracking().Where(s=>s.Mechanic.UserId==uid).OrderByDescending(s=>s.StartedAt).Select(s=>new{s.Id,s.WorkOrderId,s.StartedAt,s.PausedAt,s.EndedAt,s.PauseSeconds,s.DurationSeconds,s.Status}).ToListAsync());
    }
    public record TimerRequest(bool Resume);
    [HttpPatch("mechanic/timer/{id:guid}")]
    [Authorize(Roles="Mechanic")]
    public async Task<IActionResult> Pause(Guid id,TimerRequest input)
    {
        var uid=User.RequireUserId();
        var s=await _db.LaborSessions.FirstOrDefaultAsync(s=>s.WorkOrderId==id && s.Mechanic.UserId==uid && s.Status!=LaborSessionStatus.Ended)
            ?? throw new NotFoundException("Start a timer first.");
        if(!await _db.JobAssignments.AnyAsync(a=>a.WorkOrderId==id&&a.IsActive&&a.Mechanic.UserId==uid)) throw new ForbiddenException();
        var now=DateTime.UtcNow;
        if(input.Resume){
            if(s.Status!=LaborSessionStatus.Paused)throw new ConflictException("The timer is not paused.");
            if(await _db.LaborSessions.AnyAsync(x=>x.Id!=s.Id&&x.Mechanic.UserId==uid&&x.Status==LaborSessionStatus.Active)) throw new ConflictException("Another timer is running.");
            s.PauseSeconds+=Math.Max(0,(int)(now-s.PausedAt!.Value).TotalSeconds);s.PausedAt=null;s.Status=LaborSessionStatus.Active;
        }else{if(s.Status!=LaborSessionStatus.Active)throw new ConflictException("The timer is not running.");s.PausedAt=now;s.Status=LaborSessionStatus.Paused;}
        await _db.SaveChangesAsync();return Ok(new{s.Id,s.Status});
    }
    public record CompleteRequest([Required,StringLength(4000)]string Notes);
    [HttpPost("mechanic/jobs/{id:guid}/complete")]
    [Authorize(Roles="Mechanic")]
    public async Task<IActionResult> Complete(Guid id,CompleteRequest input,[FromServices]IMechanicService mechanic)
    {
        var mid=await mechanic.GetCurrentMechanicIdAsync(User.RequireUserId());
        // End paused sessions through the timer endpoint after resuming them to account for pause time.
        var paused=await _db.LaborSessions.AsNoTracking().FirstOrDefaultAsync(s=>s.WorkOrderId==id&&s.MechanicStaffId==mid&&s.Status==LaborSessionStatus.Paused);
        if(paused!=null)throw new ConflictException("Resume or stop the paused timer before completing the job.");
        var w=await mechanic.CompleteJobAsync(mid,id,input.Notes);
        return Ok(new{w.Id,w.WorkOrderNumber,w.Status,w.TechnicianNotes});
    }

    [HttpGet("stock-movements")]
    [Authorize(Roles="Storekeeper,Manager,Admin")]
    public async Task<IActionResult> Movements()=>Ok(await _db.StockMovements.AsNoTracking().OrderByDescending(s=>s.CreatedAt).Select(s=>new{s.Id,s.Type,s.Quantity,s.Reference,s.CreatedAt,PartName=s.Part.Name,PartNumber=s.Part.PartNumber}).ToListAsync());

    public record StaffStatusRequest([Range(0,2)]int Status);
    [HttpPatch("staff/{id:guid}/status")]
    [Authorize(Roles="Manager,Admin")]
    public async Task<IActionResult> StaffStatusUpdate(Guid id,StaffStatusRequest input)
    {
        var s=await _db.Staff.Include(s=>s.User).FirstOrDefaultAsync(s=>s.Id==id)??throw new NotFoundException("Staff member not found.");
        if(s.UserId==User.RequireUserId())throw new ConflictException("You cannot deactivate your own account.");
        if(input.Status!=0 && await _db.JobAssignments.AnyAsync(a=>a.MechanicStaffId==id&&a.IsActive))throw new ConflictException("Finish the assigned job before changing availability.");
        s.Status=(StaffStatus)input.Status;s.User.Status=input.Status==0?AccountStatus.Active:AccountStatus.Inactive;s.UpdatedAt=s.User.UpdatedAt=DateTime.UtcNow;
        await _db.SaveChangesAsync();return Ok(new{s.Id,s.Status});
    }

    public record ModificationJobRequest(Guid ServiceId);
    [HttpPost("modifications/{id:guid}/job")]
    [Authorize(Roles="ServiceAdvisor,Manager,Admin")]
    public async Task<IActionResult> ModificationJob(Guid id,ModificationJobRequest input)
    {
        var number="WO-MOD-"+id.ToString("N");
        var prior=await _db.WorkOrders.AsNoTracking().FirstOrDefaultAsync(w=>w.WorkOrderNumber==number);
        if(prior!=null)return Ok(new{prior.Id,prior.WorkOrderNumber,AlreadyExists=true});
        var m=await _db.ModificationRequests.AsNoTracking().FirstOrDefaultAsync(m=>m.Id==id)??throw new NotFoundException("Modification not found.");
        if(m.Status!=ModificationRequestStatus.Approved||m.ProposedCost==null||m.ProposedCost<0)throw new ConflictException("The customer must approve a priced quotation first.");
        if(!await _db.Services.AnyAsync(s=>s.Id==input.ServiceId&&s.IsActive))throw new ValidationException("Select an active service package.");
        if(!await _db.Vehicles.AnyAsync(v=>v.Id==m.VehicleId&&v.CustomerId==m.CustomerId))throw new ValidationException("Vehicle does not belong to this customer.");
        var description=$"Approved modification: {m.RequestType}\nAccepted price: LKR {m.ProposedCost}\nReference: {m.Id}\n{m.Description}\nAdvisor notes: {m.AdvisorNotes}";
        if(description.Length>2000)description=description[..1900]+"\nSee the modification request for full details.";
        var w=new WorkOrder{Id=Guid.NewGuid(),WorkOrderNumber=number,CustomerId=m.CustomerId,VehicleId=m.VehicleId,ServiceId=input.ServiceId,Description=description,Status=WorkOrderStatus.Open,CreatedAt=DateTime.UtcNow,UpdatedAt=DateTime.UtcNow};
        _db.WorkOrders.Add(w);
        try{await _db.SaveChangesAsync();}catch(DbUpdateException){_db.ChangeTracker.Clear();prior=await _db.WorkOrders.AsNoTracking().FirstOrDefaultAsync(x=>x.WorkOrderNumber==number);if(prior==null)throw;return Ok(new{prior.Id,prior.WorkOrderNumber,AlreadyExists=true});}
        return Ok(new{w.Id,w.WorkOrderNumber,AlreadyExists=false});
    }
    [HttpGet("mechanic/requisitions")]
    [Authorize(Roles="Mechanic")]
    public async Task<IActionResult> MyRequisitions()
    {
        var uid=User.RequireUserId();
        return Ok(await _db.PartRequisitions.AsNoTracking().Where(r=>r.RequestedBy.UserId==uid).OrderByDescending(r=>r.CreatedAt).Select(r=>new{r.Id,r.WorkOrderId,r.PartSpec,r.QtyRequested,r.QtyReleased,r.Status,r.Reason,r.CreatedAt}).ToListAsync());
    }
    public record ModificationDecision(bool Approve);
    [HttpPost("modifications/{id:guid}/decision")]
    [Authorize(Roles="Customer")]
    public async Task<IActionResult> ModificationDecisionUpdate(Guid id,ModificationDecision input)
    {
        var cid=User.RequireCustomerId();
        var m=await _db.ModificationRequests.FirstOrDefaultAsync(r=>r.Id==id&&r.CustomerId==cid)??throw new NotFoundException("Modification request not found.");
        if(m.Status!=ModificationRequestStatus.Quoted||m.ProposedCost==null||m.ProposedCost<0)throw new ConflictException("Only a priced quotation can be approved or declined.");
        m.Status=input.Approve?ModificationRequestStatus.Approved:ModificationRequestStatus.Rejected;m.UpdatedAt=DateTime.UtcNow;
        await _db.SaveChangesAsync();return Ok(new{m.Id,m.Status});
    }
    public record StockUpdate([Range(0,int.MaxValue)]int OnHandQty,[Range(0,int.MaxValue)]int ReorderLevel,[Range(0,999999999)]decimal UnitCost,[StringLength(200)]string? Location,[Required,StringLength(300)]string Reason);
    [HttpPut("inventory/{id:guid}")]
    [Authorize(Roles="Storekeeper,Manager,Admin")]
    public async Task<IActionResult> UpdateStock(Guid id,StockUpdate input)
    {
        return await _db.Database.CreateExecutionStrategy().ExecuteAsync<IActionResult>(async()=>{
            _db.ChangeTracker.Clear();
            await using var tx=await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var i=await _db.InventoryItems.FirstOrDefaultAsync(x=>x.Id==id)??throw new NotFoundException("Stock item not found.");
            if(input.OnHandQty<i.ReservedQty)throw new ValidationException("On-hand quantity cannot be lower than reserved stock.");
            var delta=input.OnHandQty-i.OnHandQty;
            if(delta!=0)_db.StockMovements.Add(new StockMovement{Id=Guid.NewGuid(),PartId=i.PartId,Type=StockMovementType.Adjustment,Quantity=delta,PerformedByUserId=User.RequireUserId(),Reference=input.Reason.Trim(),CreatedAt=DateTime.UtcNow});
            i.OnHandQty=input.OnHandQty;i.ReorderLevel=input.ReorderLevel;i.UnitCost=input.UnitCost;i.Location=input.Location;i.UpdatedAt=DateTime.UtcNow;
            await _db.SaveChangesAsync();await tx.CommitAsync();return Ok(new{i.Id,i.OnHandQty});
        });
    }
    public record QuoteEdit([Range(0.01,999999999)]decimal UnitPrice,[Range(1,int.MaxValue)]int AvailableQuantity,[Range(0,365)]int DeliveryDays,[StringLength(1000)]string? Notes);
    [HttpPut("vendor/quotes/{id:guid}")]
    [Authorize(Roles="Vendor")]
    public async Task<IActionResult> EditQuote(Guid id,QuoteEdit input)
    {
        var uid=User.RequireUserId();
        var q=await _db.VendorQuotes.Include(q=>q.QuoteRequest).FirstOrDefaultAsync(q=>q.Id==id&&q.VendorProfile.UserId==uid)??throw new NotFoundException("Quotation not found.");
        if(q.Status!=VendorQuoteStatus.Submitted||q.QuoteRequest.Status!=QuoteRequestStatus.Open)throw new ConflictException("Only a submitted quotation for an open request can be edited.");
        q.UnitPrice=input.UnitPrice;q.AvailableQuantity=input.AvailableQuantity;q.DeliveryDays=input.DeliveryDays;q.Notes=input.Notes;q.UpdatedAt=DateTime.UtcNow;
        await _db.SaveChangesAsync();return Ok(new{q.Id});
    }
    [HttpPost("vendor/quotes/{id:guid}/withdraw")]
    [Authorize(Roles="Vendor")]
    public async Task<IActionResult> WithdrawQuote(Guid id)
    {
        var uid=User.RequireUserId();
        var q=await _db.VendorQuotes.Include(q=>q.QuoteRequest).FirstOrDefaultAsync(q=>q.Id==id&&q.VendorProfile.UserId==uid)??throw new NotFoundException("Quotation not found.");
        if(q.Status!=VendorQuoteStatus.Submitted||q.QuoteRequest.Status!=QuoteRequestStatus.Open)throw new ConflictException("This quotation cannot be withdrawn.");
        q.Status=VendorQuoteStatus.Withdrawn;q.UpdatedAt=DateTime.UtcNow;await _db.SaveChangesAsync();return Ok(new{q.Id,q.Status});
    }

}
