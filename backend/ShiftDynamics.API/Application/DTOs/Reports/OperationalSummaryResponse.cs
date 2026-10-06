namespace ShiftDynamics.API.Application.DTOs.Reports;

public record OperationalSummaryResponse(DateTime From, DateTime To, WorkOrderSummary WorkOrders, BillingSummary Billing, InventorySummary Inventory, ProcurementSummary Procurement);
public record WorkOrderSummary(int Total, int Completed, int Cancelled);
public record BillingSummary(int Invoices, decimal InvoicedAmount, decimal PaidAmount, decimal OutstandingAmount);
public record InventorySummary(int LowStockItems, int PendingRequisitions);
public record ProcurementSummary(int OpenQuoteRequests, int OpenPurchaseOrders);
