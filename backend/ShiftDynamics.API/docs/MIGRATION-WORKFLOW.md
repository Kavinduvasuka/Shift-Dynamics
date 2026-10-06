# Shift Dynamics SQL Server Migration Workflow

## Prerequisites

- .NET 10 SDK
- SQL Server reachable through `ConnectionStrings:DefaultConnection`
- EF Core CLI matching the project EF Core 10 packages

Configure the connection string through User Secrets, environment variables, or a local development settings file. Do not commit credentials.

## Common commands

Run from `backend/ShiftDynamics.API`:

```powershell
dotnet ef migrations add <MigrationName> --output-dir Infrastructure/Migrations
dotnet ef database update
dotnet ef migrations script --output migration.sql
dotnet ef migrations remove
```

Review generated migrations and scripts before applying them to a shared or production database. Never edit a migration that has already been applied.

## Initial setup

```powershell
cd backend/ShiftDynamics.API
dotnet restore
dotnet ef database update
dotnet run
```

Swagger is available at the HTTPS URL printed by the app, commonly `https://localhost:7249/swagger`.

## Current provider

- Database: Microsoft SQL Server
- EF Core: 10.0
- Provider: `Microsoft.EntityFrameworkCore.SqlServer`
- Target framework: .NET 10
- Migration directory: `Infrastructure/Migrations`
