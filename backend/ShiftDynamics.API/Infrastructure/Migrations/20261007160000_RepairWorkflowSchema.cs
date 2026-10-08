using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Migrations;

[DbContext(typeof(ShiftDynamicsDbContext))]
[Migration("20261007160000_RepairWorkflowSchema")]
public class RepairWorkflowSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.parts', 'ImageUrl') IS NULL ALTER TABLE [dbo].[parts] ADD [ImageUrl] nvarchar(500) NULL;
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.modification_requests','Description') IS NULL AND COL_LENGTH('dbo.modification_requests','Request') IS NOT NULL EXEC sp_rename N'dbo.modification_requests.Request', N'Description', N'COLUMN';
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.modification_requests','AdvisorNotes') IS NULL AND COL_LENGTH('dbo.modification_requests','ReviewNotes') IS NOT NULL EXEC sp_rename N'dbo.modification_requests.ReviewNotes', N'AdvisorNotes', N'COLUMN';
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.modification_requests', 'Description') IS NULL ALTER TABLE [dbo].[modification_requests] ADD [Description] nvarchar(2000) NOT NULL CONSTRAINT DF_modification_description DEFAULT N'Legacy modification request';
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.modification_requests', 'AdvisorNotes') IS NULL ALTER TABLE [dbo].[modification_requests] ADD [AdvisorNotes] nvarchar(2000) NULL;
""");
        migrationBuilder.Sql("""
ALTER TABLE dbo.modification_requests ALTER COLUMN AdvisorNotes nvarchar(2000) NULL;
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.modification_requests', 'RequestType') IS NULL ALTER TABLE [dbo].[modification_requests] ADD [RequestType] nvarchar(100) NOT NULL CONSTRAINT DF_modification_type DEFAULT N'General modification';
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.modification_requests', 'ProposedCost') IS NULL ALTER TABLE [dbo].[modification_requests] ADD [ProposedCost] decimal(18,2) NULL;
""");
        migrationBuilder.Sql("""
UPDATE dbo.modification_requests SET UpdatedAt=CreatedAt WHERE UpdatedAt IS NULL; ALTER TABLE dbo.modification_requests ALTER COLUMN UpdatedAt datetime2 NOT NULL;
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.modification_requests','Request') IS NOT NULL
BEGIN
 EXEC(N'UPDATE dbo.modification_requests SET Description=Request WHERE Request IS NOT NULL AND (LEN(Description)=0 OR Description=N''Legacy modification request'');');
 EXEC(N'ALTER TABLE dbo.modification_requests ALTER COLUMN Request nvarchar(2000) NULL;');
END;
IF COL_LENGTH('dbo.modification_requests','ReviewNotes') IS NOT NULL
 EXEC(N'UPDATE dbo.modification_requests SET AdvisorNotes=ReviewNotes WHERE AdvisorNotes IS NULL;');
""");
        migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.quote_requests', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.quote_requests (
  Id uniqueidentifier NOT NULL CONSTRAINT PK_quote_requests PRIMARY KEY,
  RequestNumber nvarchar(50) NOT NULL, PartId uniqueidentifier NULL, PartRequisitionId uniqueidentifier NULL,
  PartDescription nvarchar(500) NOT NULL, Quantity int NOT NULL, RequiredBy datetime2 NOT NULL,
  Status nvarchar(30) NOT NULL, CreatedByUserId uniqueidentifier NOT NULL, CreatedAt datetime2 NOT NULL, UpdatedAt datetime2 NOT NULL,
  CONSTRAINT FK_quote_requests_parts_PartId FOREIGN KEY(PartId) REFERENCES dbo.parts(Id) ON DELETE SET NULL,
  CONSTRAINT FK_quote_requests_part_requisitions_PartRequisitionId FOREIGN KEY(PartRequisitionId) REFERENCES dbo.part_requisitions(Id) ON DELETE SET NULL
 );
 CREATE UNIQUE INDEX IX_quote_requests_RequestNumber ON dbo.quote_requests(RequestNumber);
 CREATE INDEX IX_quote_requests_PartId ON dbo.quote_requests(PartId);
 CREATE INDEX IX_quote_requests_PartRequisitionId ON dbo.quote_requests(PartRequisitionId);
END;
""");
        migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.vendor_quote_requests', N'U') IS NOT NULL
BEGIN
 INSERT dbo.quote_requests (Id,RequestNumber,PartId,PartRequisitionId,PartDescription,Quantity,RequiredBy,Status,CreatedByUserId,CreatedAt,UpdatedAt)
 SELECT r.Id, N'Legacy-'+CONVERT(nvarchar(36),r.Id),r.PartId,NULL,
 LEFT(COALESCE(NULLIF(r.Specifications,N''),p.Name,N'Legacy part request'),500),r.Quantity,r.CreatedAt,
 CASE WHEN r.Status IN ('Open','Closed','Awarded','Cancelled') THEN r.Status ELSE 'Closed' END,r.RequestedByUserId,r.CreatedAt,COALESCE(r.ClosedAt,r.CreatedAt)
 FROM dbo.vendor_quote_requests r LEFT JOIN dbo.parts p ON p.Id=r.PartId
 WHERE NOT EXISTS(SELECT 1 FROM dbo.quote_requests n WHERE n.Id=r.Id);
END;
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.vendor_quotes', 'AvailableQuantity') IS NULL ALTER TABLE [dbo].[vendor_quotes] ADD [AvailableQuantity] int NULL;
""");
        migrationBuilder.Sql("""
UPDATE v SET AvailableQuantity=q.Quantity FROM dbo.vendor_quotes v JOIN dbo.quote_requests q ON q.Id=v.QuoteRequestId WHERE v.AvailableQuantity IS NULL; ALTER TABLE dbo.vendor_quotes ALTER COLUMN AvailableQuantity int NOT NULL;
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.vendor_quotes', 'UpdatedAt') IS NULL ALTER TABLE [dbo].[vendor_quotes] ADD [UpdatedAt] datetime2 NULL;
""");
        migrationBuilder.Sql("""
UPDATE dbo.vendor_quotes SET UpdatedAt=SubmittedAt WHERE UpdatedAt IS NULL; ALTER TABLE dbo.vendor_quotes ALTER COLUMN UpdatedAt datetime2 NOT NULL;
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.purchase_orders','PurchaseOrderNumber') IS NULL AND COL_LENGTH('dbo.purchase_orders','OrderNumber') IS NOT NULL EXEC sp_rename N'dbo.purchase_orders.OrderNumber', N'PurchaseOrderNumber', N'COLUMN';
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.purchase_orders','ApprovedByUserId') IS NULL AND COL_LENGTH('dbo.purchase_orders','CreatedByUserId') IS NOT NULL EXEC sp_rename N'dbo.purchase_orders.CreatedByUserId', N'ApprovedByUserId', N'COLUMN';
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.purchase_orders','ApprovedAt') IS NULL AND COL_LENGTH('dbo.purchase_orders','CreatedAt') IS NOT NULL EXEC sp_rename N'dbo.purchase_orders.CreatedAt', N'ApprovedAt', N'COLUMN';
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.purchase_orders', 'ExpectedDeliveryAt') IS NULL ALTER TABLE [dbo].[purchase_orders] ADD [ExpectedDeliveryAt] datetime2 NULL;
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.purchase_orders', 'ReceivedQuantity') IS NULL ALTER TABLE [dbo].[purchase_orders] ADD [ReceivedQuantity] int NOT NULL CONSTRAINT DF_purchase_received_fix DEFAULT 0;
""");
        migrationBuilder.Sql("""
UPDATE dbo.purchase_orders SET ReceivedQuantity=Quantity WHERE Status='Received' AND ReceivedQuantity=0;
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.purchase_orders','PartId') IS NOT NULL EXEC(N'ALTER TABLE dbo.purchase_orders ALTER COLUMN [PartId] uniqueidentifier NULL;');
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.purchase_orders','VendorProfileId') IS NOT NULL EXEC(N'ALTER TABLE dbo.purchase_orders ALTER COLUMN [VendorProfileId] uniqueidentifier NULL;');
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.purchase_orders','TotalAmount') IS NOT NULL EXEC(N'ALTER TABLE dbo.purchase_orders ALTER COLUMN [TotalAmount] decimal(18,2) NULL;');
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.purchase_orders','CreatedByUserId') IS NOT NULL EXEC(N'ALTER TABLE dbo.purchase_orders ALTER COLUMN [CreatedByUserId] uniqueidentifier NULL;');
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.purchase_orders','CreatedAt') IS NOT NULL EXEC(N'ALTER TABLE dbo.purchase_orders ALTER COLUMN [CreatedAt] datetime2 NULL;');
""");
        migrationBuilder.Sql("""
-- A legacy OrderNumber index is handled in the next command before changing nullability.
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.purchase_orders','OrderNumber') IS NOT NULL
BEGIN
 DECLARE @sql nvarchar(max)=N'';
 SELECT @sql=@sql+N'DROP INDEX '+QUOTENAME(i.name)+N' ON dbo.purchase_orders;'
 FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
 JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
 WHERE i.object_id=OBJECT_ID('dbo.purchase_orders') AND i.is_unique=1 AND i.is_primary_key=0 AND c.name='OrderNumber';
 EXEC sp_executesql @sql;
 EXEC(N'ALTER TABLE dbo.purchase_orders ALTER COLUMN OrderNumber nvarchar(50) NULL;');
 EXEC(N'UPDATE dbo.purchase_orders SET PurchaseOrderNumber=OrderNumber WHERE PurchaseOrderNumber IS NULL OR LEN(PurchaseOrderNumber)=0;');
END;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.purchase_orders') AND name='IX_purchase_orders_PurchaseOrderNumber')
 CREATE UNIQUE INDEX IX_purchase_orders_PurchaseOrderNumber ON dbo.purchase_orders(PurchaseOrderNumber);
""");
        migrationBuilder.Sql("""
DECLARE @sql nvarchar(max)=N'';
 SELECT @sql=@sql+N'ALTER TABLE dbo.vendor_quotes DROP CONSTRAINT '+QUOTENAME(f.name)+N';'
 FROM sys.foreign_keys f JOIN sys.foreign_key_columns fc ON fc.constraint_object_id=f.object_id
 JOIN sys.columns c ON c.object_id=fc.parent_object_id AND c.column_id=fc.parent_column_id
 WHERE f.parent_object_id=OBJECT_ID('dbo.vendor_quotes') AND c.name='QuoteRequestId';
 EXEC sp_executesql @sql;
 ALTER TABLE dbo.vendor_quotes WITH CHECK ADD CONSTRAINT FK_vendor_quotes_quote_requests_QuoteRequestId FOREIGN KEY([QuoteRequestId]) REFERENCES dbo.quote_requests(Id);
""");
        migrationBuilder.Sql("""
DECLARE @sql nvarchar(max)=N'';
 SELECT @sql=@sql+N'ALTER TABLE dbo.vendor_quotes DROP CONSTRAINT '+QUOTENAME(f.name)+N';'
 FROM sys.foreign_keys f JOIN sys.foreign_key_columns fc ON fc.constraint_object_id=f.object_id
 JOIN sys.columns c ON c.object_id=fc.parent_object_id AND c.column_id=fc.parent_column_id
 WHERE f.parent_object_id=OBJECT_ID('dbo.vendor_quotes') AND c.name='VendorProfileId';
 EXEC sp_executesql @sql;
 ALTER TABLE dbo.vendor_quotes WITH CHECK ADD CONSTRAINT FK_vendor_quotes_vendor_profiles_VendorProfileId FOREIGN KEY([VendorProfileId]) REFERENCES dbo.vendor_profiles(Id);
""");
        migrationBuilder.Sql("""
DECLARE @sql nvarchar(max)=N'';
 SELECT @sql=@sql+N'ALTER TABLE dbo.purchase_orders DROP CONSTRAINT '+QUOTENAME(f.name)+N';'
 FROM sys.foreign_keys f JOIN sys.foreign_key_columns fc ON fc.constraint_object_id=f.object_id
 JOIN sys.columns c ON c.object_id=fc.parent_object_id AND c.column_id=fc.parent_column_id
 WHERE f.parent_object_id=OBJECT_ID('dbo.purchase_orders') AND c.name='QuoteRequestId';
 EXEC sp_executesql @sql;
 ALTER TABLE dbo.purchase_orders WITH CHECK ADD CONSTRAINT FK_purchase_orders_quote_requests_QuoteRequestId FOREIGN KEY([QuoteRequestId]) REFERENCES dbo.quote_requests(Id);
""");
        migrationBuilder.Sql("""
DECLARE @sql nvarchar(max)=N'';
 SELECT @sql=@sql+N'ALTER TABLE dbo.purchase_orders DROP CONSTRAINT '+QUOTENAME(f.name)+N';'
 FROM sys.foreign_keys f JOIN sys.foreign_key_columns fc ON fc.constraint_object_id=f.object_id
 JOIN sys.columns c ON c.object_id=fc.parent_object_id AND c.column_id=fc.parent_column_id
 WHERE f.parent_object_id=OBJECT_ID('dbo.purchase_orders') AND c.name='VendorQuoteId';
 EXEC sp_executesql @sql;
 ALTER TABLE dbo.purchase_orders WITH CHECK ADD CONSTRAINT FK_purchase_orders_vendor_quotes_VendorQuoteId FOREIGN KEY([VendorQuoteId]) REFERENCES dbo.vendor_quotes(Id);
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.DiagnosticFindings','MechanicId') IS NOT NULL
BEGIN
 EXEC(N'UPDATE r SET MechanicStaffId=r.MechanicId FROM dbo.DiagnosticFindings r WHERE NOT EXISTS(SELECT 1 FROM dbo.staff s WHERE s.Id=r.MechanicStaffId) AND EXISTS(SELECT 1 FROM dbo.staff s WHERE s.Id=r.MechanicId);');
 DECLARE @sql nvarchar(max)=N'';
 SELECT @sql=@sql+N'ALTER TABLE dbo.DiagnosticFindings DROP CONSTRAINT '+QUOTENAME(f.name)+N';'
 FROM sys.foreign_keys f JOIN sys.foreign_key_columns fc ON fc.constraint_object_id=f.object_id
 JOIN sys.columns c ON c.object_id=fc.parent_object_id AND c.column_id=fc.parent_column_id
 WHERE f.parent_object_id=OBJECT_ID('dbo.DiagnosticFindings') AND c.name='MechanicId';
 EXEC sp_executesql @sql;
 SET @sql=N'';
 SELECT @sql=@sql+N'DROP INDEX '+QUOTENAME(i.name)+N' ON dbo.DiagnosticFindings;'
 FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
 JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
 WHERE i.object_id=OBJECT_ID('dbo.DiagnosticFindings') AND c.name='MechanicId';
 EXEC sp_executesql @sql;
 EXEC(N'ALTER TABLE dbo.DiagnosticFindings DROP COLUMN MechanicId;');
END;
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('dbo.DiagnosticFindings') AND name='FK_DiagnosticFindings_staff_MechanicStaffId')
 ALTER TABLE dbo.DiagnosticFindings WITH CHECK ADD CONSTRAINT FK_DiagnosticFindings_staff_MechanicStaffId FOREIGN KEY(MechanicStaffId) REFERENCES dbo.staff(Id);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.DiagnosticFindings') AND name='IX_DiagnosticFindings_MechanicStaffId')
 CREATE INDEX IX_DiagnosticFindings_MechanicStaffId ON dbo.DiagnosticFindings(MechanicStaffId);
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.RepairActions','MechanicId') IS NOT NULL
BEGIN
 EXEC(N'UPDATE r SET MechanicStaffId=r.MechanicId FROM dbo.RepairActions r WHERE NOT EXISTS(SELECT 1 FROM dbo.staff s WHERE s.Id=r.MechanicStaffId) AND EXISTS(SELECT 1 FROM dbo.staff s WHERE s.Id=r.MechanicId);');
 DECLARE @sql nvarchar(max)=N'';
 SELECT @sql=@sql+N'ALTER TABLE dbo.RepairActions DROP CONSTRAINT '+QUOTENAME(f.name)+N';'
 FROM sys.foreign_keys f JOIN sys.foreign_key_columns fc ON fc.constraint_object_id=f.object_id
 JOIN sys.columns c ON c.object_id=fc.parent_object_id AND c.column_id=fc.parent_column_id
 WHERE f.parent_object_id=OBJECT_ID('dbo.RepairActions') AND c.name='MechanicId';
 EXEC sp_executesql @sql;
 SET @sql=N'';
 SELECT @sql=@sql+N'DROP INDEX '+QUOTENAME(i.name)+N' ON dbo.RepairActions;'
 FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
 JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
 WHERE i.object_id=OBJECT_ID('dbo.RepairActions') AND c.name='MechanicId';
 EXEC sp_executesql @sql;
 EXEC(N'ALTER TABLE dbo.RepairActions DROP COLUMN MechanicId;');
END;
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('dbo.RepairActions') AND name='FK_RepairActions_staff_MechanicStaffId')
 ALTER TABLE dbo.RepairActions WITH CHECK ADD CONSTRAINT FK_RepairActions_staff_MechanicStaffId FOREIGN KEY(MechanicStaffId) REFERENCES dbo.staff(Id);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.RepairActions') AND name='IX_RepairActions_MechanicStaffId')
 CREATE INDEX IX_RepairActions_MechanicStaffId ON dbo.RepairActions(MechanicStaffId);
""");
        migrationBuilder.Sql("""
IF COL_LENGTH('dbo.MechanicRecommendations','MechanicId') IS NOT NULL
BEGIN
 EXEC(N'UPDATE r SET MechanicStaffId=r.MechanicId FROM dbo.MechanicRecommendations r WHERE NOT EXISTS(SELECT 1 FROM dbo.staff s WHERE s.Id=r.MechanicStaffId) AND EXISTS(SELECT 1 FROM dbo.staff s WHERE s.Id=r.MechanicId);');
 DECLARE @sql nvarchar(max)=N'';
 SELECT @sql=@sql+N'ALTER TABLE dbo.MechanicRecommendations DROP CONSTRAINT '+QUOTENAME(f.name)+N';'
 FROM sys.foreign_keys f JOIN sys.foreign_key_columns fc ON fc.constraint_object_id=f.object_id
 JOIN sys.columns c ON c.object_id=fc.parent_object_id AND c.column_id=fc.parent_column_id
 WHERE f.parent_object_id=OBJECT_ID('dbo.MechanicRecommendations') AND c.name='MechanicId';
 EXEC sp_executesql @sql;
 SET @sql=N'';
 SELECT @sql=@sql+N'DROP INDEX '+QUOTENAME(i.name)+N' ON dbo.MechanicRecommendations;'
 FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
 JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
 WHERE i.object_id=OBJECT_ID('dbo.MechanicRecommendations') AND c.name='MechanicId';
 EXEC sp_executesql @sql;
 EXEC(N'ALTER TABLE dbo.MechanicRecommendations DROP COLUMN MechanicId;');
END;
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('dbo.MechanicRecommendations') AND name='FK_MechanicRecommendations_staff_MechanicStaffId')
 ALTER TABLE dbo.MechanicRecommendations WITH CHECK ADD CONSTRAINT FK_MechanicRecommendations_staff_MechanicStaffId FOREIGN KEY(MechanicStaffId) REFERENCES dbo.staff(Id);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.MechanicRecommendations') AND name='IX_MechanicRecommendations_MechanicStaffId')
 CREATE INDEX IX_MechanicRecommendations_MechanicStaffId ON dbo.MechanicRecommendations(MechanicStaffId);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException("This data-preserving schema repair cannot be reversed automatically. Restore a database backup to roll back.");
    }
}