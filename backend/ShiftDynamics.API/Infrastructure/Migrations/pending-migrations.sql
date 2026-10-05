IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [contact_inquiries] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Phone] nvarchar(max) NULL,
        [Type] nvarchar(max) NULL,
        [Subject] nvarchar(300) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ReadAt] datetime2 NULL,
        [ResolvedAt] datetime2 NULL,
        [ResolvedByUserId] uniqueidentifier NULL,
        CONSTRAINT [PK_contact_inquiries] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [customers] (
        [Id] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Phone] nvarchar(30) NOT NULL,
        [Email] nvarchar(255) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_customers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [emergency_service_providers] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Category] nvarchar(50) NOT NULL,
        [Phone] nvarchar(30) NOT NULL,
        [Address] nvarchar(max) NULL,
        [Latitude] decimal(9,6) NOT NULL,
        [Longitude] decimal(9,6) NOT NULL,
        [OpeningHours] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_emergency_service_providers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [parts] (
        [Id] uniqueidentifier NOT NULL,
        [PartNumber] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Category] nvarchar(max) NULL,
        [Compatibility] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_parts] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [services] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [BasePrice] decimal(12,2) NOT NULL,
        [EstimatedDurationMinutes] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_services] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [vendor_registrations] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessName] nvarchar(200) NOT NULL,
        [ContactPerson] nvarchar(max) NOT NULL,
        [Mobile] nvarchar(max) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Address] nvarchar(max) NULL,
        [Specialization] nvarchar(max) NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [ReviewedByUserId] uniqueidentifier NULL,
        [ReviewedAt] datetime2 NULL,
        [RejectionReason] nvarchar(max) NULL,
        [CreatedUserId] uniqueidentifier NULL,
        CONSTRAINT [PK_vendor_registrations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [workshop_bays] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_workshop_bays] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [users] (
        [Id] uniqueidentifier NOT NULL,
        [FullName] nvarchar(150) NOT NULL,
        [Email] nvarchar(191) NOT NULL,
        [Phone] nvarchar(30) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [Role] nvarchar(50) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CustomerId] uniqueidentifier NULL,
        CONSTRAINT [PK_users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_users_customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [customers] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [vehicles] (
        [Id] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [RegistrationNumber] nvarchar(30) NOT NULL,
        [Make] nvarchar(100) NOT NULL,
        [Model] nvarchar(100) NOT NULL,
        [Year] int NOT NULL,
        [VIN] nvarchar(50) NULL,
        [Color] nvarchar(50) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_vehicles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_vehicles_customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [customers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [inventory_items] (
        [Id] uniqueidentifier NOT NULL,
        [PartId] uniqueidentifier NOT NULL,
        [OnHandQty] int NOT NULL,
        [ReservedQty] int NOT NULL,
        [ReorderLevel] int NOT NULL,
        [UnitCost] decimal(18,2) NOT NULL,
        [Location] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_inventory_items] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_inventory_items_non_negative] CHECK ("OnHandQty" >= 0 AND "ReservedQty" >= 0 AND "ReorderLevel" >= 0 AND "UnitCost" >= 0),
        CONSTRAINT [FK_inventory_items_parts_PartId] FOREIGN KEY ([PartId]) REFERENCES [parts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [stock_movements] (
        [Id] uniqueidentifier NOT NULL,
        [PartId] uniqueidentifier NOT NULL,
        [RequisitionId] uniqueidentifier NULL,
        [Type] nvarchar(30) NOT NULL,
        [Quantity] int NOT NULL,
        [PerformedByUserId] uniqueidentifier NOT NULL,
        [Reference] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_stock_movements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_stock_movements_parts_PartId] FOREIGN KEY ([PartId]) REFERENCES [parts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [EntityType] nvarchar(max) NULL,
        [EntityId] uniqueidentifier NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_notifications_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [password_reset_tokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [TokenHash] nvarchar(128) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [UsedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_password_reset_tokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_password_reset_tokens_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [staff] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [EmployeeNumber] nvarchar(50) NOT NULL,
        [Role] nvarchar(50) NOT NULL,
        [Specialization] nvarchar(150) NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_staff] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_staff_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [vendor_profiles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [RegistrationId] uniqueidentifier NULL,
        [BusinessName] nvarchar(200) NOT NULL,
        [ContactPerson] nvarchar(150) NOT NULL,
        [Mobile] nvarchar(30) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Address] nvarchar(500) NULL,
        [Specialization] nvarchar(200) NULL,
        [ApprovalStatus] nvarchar(30) NOT NULL,
        [ApprovedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_vendor_profiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_vendor_profiles_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_vendor_profiles_vendor_registrations_RegistrationId] FOREIGN KEY ([RegistrationId]) REFERENCES [vendor_registrations] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [appointments] (
        [Id] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [VehicleId] uniqueidentifier NOT NULL,
        [AppointmentDate] datetime2 NOT NULL,
        [ServiceType] nvarchar(150) NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_appointments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_appointments_customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_appointments_vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [vehicles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [emergency_requests] (
        [Id] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [VehicleId] uniqueidentifier NULL,
        [Location] nvarchar(500) NOT NULL,
        [Latitude] decimal(10,7) NULL,
        [Longitude] decimal(10,7) NULL,
        [ProblemDescription] nvarchar(2000) NOT NULL,
        [Status] int NOT NULL,
        [AssignedStaffId] uniqueidentifier NULL,
        [RequestedAt] datetime2 NOT NULL,
        [AcceptedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_emergency_requests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_emergency_requests_customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_emergency_requests_staff_AssignedStaffId] FOREIGN KEY ([AssignedStaffId]) REFERENCES [staff] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_emergency_requests_vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [vehicles] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [work_orders] (
        [Id] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [VehicleId] uniqueidentifier NOT NULL,
        [AppointmentId] uniqueidentifier NULL,
        [ServiceId] uniqueidentifier NOT NULL,
        [AssignedStaffId] uniqueidentifier NULL,
        [WorkOrderNumber] nvarchar(50) NOT NULL,
        [Status] int NOT NULL,
        [Description] nvarchar(2000) NULL,
        [TechnicianNotes] nvarchar(4000) NULL,
        [StartedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_work_orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_work_orders_appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [appointments] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_work_orders_customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_work_orders_services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [services] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_work_orders_staff_AssignedStaffId] FOREIGN KEY ([AssignedStaffId]) REFERENCES [staff] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_work_orders_vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [vehicles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [estimates] (
        [Id] uniqueidentifier NOT NULL,
        [WorkOrderId] uniqueidentifier NOT NULL,
        [EstimateNumber] nvarchar(50) NOT NULL,
        [LaborCost] decimal(12,2) NOT NULL,
        [PartsCost] decimal(12,2) NOT NULL,
        [Subtotal] decimal(12,2) NOT NULL,
        [TaxAmount] decimal(12,2) NOT NULL,
        [DiscountAmount] decimal(12,2) NOT NULL,
        [TotalAmount] decimal(12,2) NOT NULL,
        [Status] int NOT NULL,
        [CustomerApproved] bit NOT NULL,
        [ApprovedAt] datetime2 NULL,
        [Notes] nvarchar(2000) NULL,
        [ExpiresAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_estimates] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_estimates_non_negative] CHECK ("LaborCost" >= 0 AND "PartsCost" >= 0 AND "TaxAmount" >= 0 AND "DiscountAmount" >= 0 AND "Subtotal" >= 0 AND "TotalAmount" >= 0),
        CONSTRAINT [FK_estimates_work_orders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [work_orders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [job_assignments] (
        [Id] uniqueidentifier NOT NULL,
        [WorkOrderId] uniqueidentifier NOT NULL,
        [MechanicStaffId] uniqueidentifier NOT NULL,
        [BayId] uniqueidentifier NULL,
        [AssignedByUserId] uniqueidentifier NOT NULL,
        [AssignedAt] datetime2 NOT NULL,
        [EndedAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_job_assignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_job_assignments_staff_MechanicStaffId] FOREIGN KEY ([MechanicStaffId]) REFERENCES [staff] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_job_assignments_work_orders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [work_orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_job_assignments_workshop_bays_BayId] FOREIGN KEY ([BayId]) REFERENCES [workshop_bays] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [labor_sessions] (
        [Id] uniqueidentifier NOT NULL,
        [WorkOrderId] uniqueidentifier NOT NULL,
        [MechanicStaffId] uniqueidentifier NOT NULL,
        [StartedAt] datetime2 NOT NULL,
        [PausedAt] datetime2 NULL,
        [EndedAt] datetime2 NULL,
        [PauseSeconds] int NOT NULL,
        [DurationSeconds] int NULL,
        [Status] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_labor_sessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_labor_sessions_staff_MechanicStaffId] FOREIGN KEY ([MechanicStaffId]) REFERENCES [staff] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_labor_sessions_work_orders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [work_orders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [part_requisitions] (
        [Id] uniqueidentifier NOT NULL,
        [WorkOrderId] uniqueidentifier NOT NULL,
        [RequestedByStaffId] uniqueidentifier NOT NULL,
        [PartId] uniqueidentifier NULL,
        [PartSpec] nvarchar(500) NOT NULL,
        [QtyRequested] int NOT NULL,
        [QtyReleased] int NOT NULL,
        [Urgency] nvarchar(30) NOT NULL,
        [Reason] nvarchar(max) NULL,
        [Status] nvarchar(30) NOT NULL,
        [ReviewedByUserId] uniqueidentifier NULL,
        [ReviewedAt] datetime2 NULL,
        [ReviewNotes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_part_requisitions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_part_requisitions_parts_PartId] FOREIGN KEY ([PartId]) REFERENCES [parts] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_part_requisitions_staff_RequestedByStaffId] FOREIGN KEY ([RequestedByStaffId]) REFERENCES [staff] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_part_requisitions_work_orders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [work_orders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [invoices] (
        [Id] uniqueidentifier NOT NULL,
        [WorkOrderId] uniqueidentifier NOT NULL,
        [EstimateId] uniqueidentifier NULL,
        [InvoiceNumber] nvarchar(50) NOT NULL,
        [LaborCost] decimal(12,2) NOT NULL,
        [PartsCost] decimal(12,2) NOT NULL,
        [Subtotal] decimal(12,2) NOT NULL,
        [TaxAmount] decimal(12,2) NOT NULL,
        [DiscountAmount] decimal(12,2) NOT NULL,
        [TotalAmount] decimal(12,2) NOT NULL,
        [AmountPaid] decimal(12,2) NOT NULL,
        [BalanceDue] decimal(12,2) NOT NULL,
        [Status] int NOT NULL,
        [IssuedAt] datetime2 NULL,
        [DueDate] datetime2 NULL,
        [Notes] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_invoices] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_invoices_non_negative] CHECK ("LaborCost" >= 0 AND "PartsCost" >= 0 AND "TaxAmount" >= 0 AND "DiscountAmount" >= 0 AND "Subtotal" >= 0 AND "TotalAmount" >= 0 AND "AmountPaid" >= 0 AND "BalanceDue" >= 0 AND "AmountPaid" <= "TotalAmount"),
        CONSTRAINT [FK_invoices_estimates_EstimateId] FOREIGN KEY ([EstimateId]) REFERENCES [estimates] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_invoices_work_orders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [work_orders] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [payments] (
        [Id] uniqueidentifier NOT NULL,
        [InvoiceId] uniqueidentifier NOT NULL,
        [Amount] decimal(12,2) NOT NULL,
        [Method] int NOT NULL,
        [Status] int NOT NULL,
        [TransactionReference] nvarchar(150) NULL,
        [PaymentDate] datetime2 NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_payments_invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [invoices] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_appointments_AppointmentDate] ON [appointments] ([AppointmentDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_appointments_CustomerId] ON [appointments] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_appointments_VehicleId] ON [appointments] ([VehicleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_customers_Email] ON [customers] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_customers_Phone] ON [customers] ([Phone]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_emergency_requests_AssignedStaffId] ON [emergency_requests] ([AssignedStaffId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_emergency_requests_CustomerId] ON [emergency_requests] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_emergency_requests_Status] ON [emergency_requests] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_emergency_requests_VehicleId] ON [emergency_requests] ([VehicleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_estimates_EstimateNumber] ON [estimates] ([EstimateNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_estimates_WorkOrderId] ON [estimates] ([WorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_inventory_items_PartId] ON [inventory_items] ([PartId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_invoices_EstimateId] ON [invoices] ([EstimateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_invoices_InvoiceNumber] ON [invoices] ([InvoiceNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_invoices_WorkOrderId] ON [invoices] ([WorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_job_assignments_BayId_IsActive] ON [job_assignments] ([BayId], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_job_assignments_MechanicStaffId_IsActive] ON [job_assignments] ([MechanicStaffId], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_job_assignments_WorkOrderId] ON [job_assignments] ([WorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_labor_sessions_MechanicStaffId] ON [labor_sessions] ([MechanicStaffId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_labor_sessions_WorkOrderId] ON [labor_sessions] ([WorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_notifications_UserId_IsRead] ON [notifications] ([UserId], [IsRead]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_part_requisitions_PartId] ON [part_requisitions] ([PartId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_part_requisitions_RequestedByStaffId] ON [part_requisitions] ([RequestedByStaffId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_part_requisitions_WorkOrderId] ON [part_requisitions] ([WorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_parts_PartNumber] ON [parts] ([PartNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_password_reset_tokens_TokenHash] ON [password_reset_tokens] ([TokenHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_password_reset_tokens_UserId] ON [password_reset_tokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_payments_InvoiceId] ON [payments] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_payments_TransactionReference] ON [payments] ([TransactionReference]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_services_Name] ON [services] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_staff_EmployeeNumber] ON [staff] ([EmployeeNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_staff_UserId] ON [staff] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_stock_movements_PartId] ON [stock_movements] ([PartId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_users_CustomerId] ON [users] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_users_Email] ON [users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_users_Phone] ON [users] ([Phone]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_vehicles_CustomerId] ON [vehicles] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_vehicles_RegistrationNumber] ON [vehicles] ([RegistrationNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_vehicles_VIN] ON [vehicles] ([VIN]) WHERE [VIN] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_vendor_profiles_Email] ON [vendor_profiles] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_vendor_profiles_RegistrationId] ON [vendor_profiles] ([RegistrationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_vendor_profiles_UserId] ON [vendor_profiles] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_vendor_registrations_Email] ON [vendor_registrations] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_work_orders_AppointmentId] ON [work_orders] ([AppointmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_work_orders_AssignedStaffId] ON [work_orders] ([AssignedStaffId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_work_orders_CustomerId] ON [work_orders] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_work_orders_ServiceId] ON [work_orders] ([ServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_work_orders_VehicleId] ON [work_orders] ([VehicleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_work_orders_WorkOrderNumber] ON [work_orders] ([WorkOrderNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_workshop_bays_Name] ON [workshop_bays] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917094831_InitialSqlServerSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260917094831_InitialSqlServerSchema', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929164336_AddMechanicWorkflowRecords'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929164336_AddMechanicWorkflowRecords', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE TABLE [DiagnosticFindings] (
        [Id] uniqueidentifier NOT NULL,
        [WorkOrderId] uniqueidentifier NOT NULL,
        [MechanicStaffId] uniqueidentifier NOT NULL,
        [Finding] nvarchar(max) NOT NULL,
        [Severity] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [MechanicId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_DiagnosticFindings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DiagnosticFindings_staff_MechanicId] FOREIGN KEY ([MechanicId]) REFERENCES [staff] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DiagnosticFindings_work_orders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [work_orders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE TABLE [MechanicRecommendations] (
        [Id] uniqueidentifier NOT NULL,
        [WorkOrderId] uniqueidentifier NOT NULL,
        [MechanicStaffId] uniqueidentifier NOT NULL,
        [Recommendation] nvarchar(max) NOT NULL,
        [Priority] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [MechanicId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_MechanicRecommendations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MechanicRecommendations_staff_MechanicId] FOREIGN KEY ([MechanicId]) REFERENCES [staff] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MechanicRecommendations_work_orders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [work_orders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE TABLE [modification_requests] (
        [Id] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [VehicleId] uniqueidentifier NOT NULL,
        [RequestType] nvarchar(100) NOT NULL,
        [Description] nvarchar(2000) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [ProposedCost] decimal(18,2) NULL,
        [AdvisorNotes] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_modification_requests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_modification_requests_customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [customers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_modification_requests_vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [vehicles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE TABLE [quote_requests] (
        [Id] uniqueidentifier NOT NULL,
        [RequestNumber] nvarchar(50) NOT NULL,
        [PartId] uniqueidentifier NULL,
        [PartRequisitionId] uniqueidentifier NULL,
        [PartDescription] nvarchar(500) NOT NULL,
        [Quantity] int NOT NULL,
        [RequiredBy] datetime2 NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_quote_requests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_quote_requests_part_requisitions_PartRequisitionId] FOREIGN KEY ([PartRequisitionId]) REFERENCES [part_requisitions] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_quote_requests_parts_PartId] FOREIGN KEY ([PartId]) REFERENCES [parts] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE TABLE [RepairActions] (
        [Id] uniqueidentifier NOT NULL,
        [WorkOrderId] uniqueidentifier NOT NULL,
        [MechanicStaffId] uniqueidentifier NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [MechanicId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_RepairActions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RepairActions_staff_MechanicId] FOREIGN KEY ([MechanicId]) REFERENCES [staff] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RepairActions_work_orders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [work_orders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE TABLE [vendor_quotes] (
        [Id] uniqueidentifier NOT NULL,
        [QuoteRequestId] uniqueidentifier NOT NULL,
        [VendorProfileId] uniqueidentifier NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [AvailableQuantity] int NOT NULL,
        [DeliveryDays] int NOT NULL,
        [Notes] nvarchar(max) NULL,
        [Status] nvarchar(30) NOT NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_vendor_quotes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_vendor_quotes_quote_requests_QuoteRequestId] FOREIGN KEY ([QuoteRequestId]) REFERENCES [quote_requests] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_vendor_quotes_vendor_profiles_VendorProfileId] FOREIGN KEY ([VendorProfileId]) REFERENCES [vendor_profiles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE TABLE [purchase_orders] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseOrderNumber] nvarchar(50) NOT NULL,
        [VendorQuoteId] uniqueidentifier NOT NULL,
        [QuoteRequestId] uniqueidentifier NOT NULL,
        [ApprovedByUserId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [ApprovedAt] datetime2 NOT NULL,
        [ExpectedDeliveryAt] datetime2 NULL,
        [ReceivedAt] datetime2 NULL,
        CONSTRAINT [PK_purchase_orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_purchase_orders_quote_requests_QuoteRequestId] FOREIGN KEY ([QuoteRequestId]) REFERENCES [quote_requests] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_purchase_orders_vendor_quotes_VendorQuoteId] FOREIGN KEY ([VendorQuoteId]) REFERENCES [vendor_quotes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE INDEX [IX_DiagnosticFindings_MechanicId] ON [DiagnosticFindings] ([MechanicId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE INDEX [IX_DiagnosticFindings_WorkOrderId] ON [DiagnosticFindings] ([WorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE INDEX [IX_MechanicRecommendations_MechanicId] ON [MechanicRecommendations] ([MechanicId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE INDEX [IX_MechanicRecommendations_WorkOrderId] ON [MechanicRecommendations] ([WorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE INDEX [IX_modification_requests_CustomerId] ON [modification_requests] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE INDEX [IX_modification_requests_VehicleId] ON [modification_requests] ([VehicleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE UNIQUE INDEX [IX_purchase_orders_PurchaseOrderNumber] ON [purchase_orders] ([PurchaseOrderNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE INDEX [IX_purchase_orders_QuoteRequestId] ON [purchase_orders] ([QuoteRequestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE UNIQUE INDEX [IX_purchase_orders_VendorQuoteId] ON [purchase_orders] ([VendorQuoteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE INDEX [IX_quote_requests_PartId] ON [quote_requests] ([PartId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE INDEX [IX_quote_requests_PartRequisitionId] ON [quote_requests] ([PartRequisitionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE UNIQUE INDEX [IX_quote_requests_RequestNumber] ON [quote_requests] ([RequestNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE INDEX [IX_RepairActions_MechanicId] ON [RepairActions] ([MechanicId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE INDEX [IX_RepairActions_WorkOrderId] ON [RepairActions] ([WorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE UNIQUE INDEX [IX_vendor_quotes_QuoteRequestId_VendorProfileId] ON [vendor_quotes] ([QuoteRequestId], [VendorProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    CREATE INDEX [IX_vendor_quotes_VendorProfileId] ON [vendor_quotes] ([VendorProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182339_AddProcurementNotificationsAndModifications'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005182339_AddProcurementNotificationsAndModifications', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182634_AddPurchaseOrderReceiptTracking'
)
BEGIN
    ALTER TABLE [purchase_orders] ADD [ReceivedQuantity] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005182634_AddPurchaseOrderReceiptTracking'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005182634_AddPurchaseOrderReceiptTracking', N'10.0.0');
END;

COMMIT;
GO

