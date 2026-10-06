namespace ShiftDynamics.API.Application.DTOs.Invoices;

public record CreateInvoiceRequest(Guid WorkOrderId, Guid? EstimateId, decimal LaborCost, decimal PartsCost, decimal TaxAmount = 0, decimal DiscountAmount = 0, string? Notes = null);
