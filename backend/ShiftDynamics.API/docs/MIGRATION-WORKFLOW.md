# Shift Dynamics - Database Migration Workflow

## Prerequisites

- .NET 10 SDK
- MySQL 8.x running locally (or reachable via connection string)
- Pomelo.EntityFrameworkCore.MySql
- Connection string configured via appsettings.Development.json, User Secrets, or the ConnectionStrings__DefaultConnection environment variable.

Example MySQL connection string:

    Server=localhost;Port=3306;Database=shift_dynamics;User=root;Password=...

## Common commands

Run from backend/ShiftDynamics.API:

    dotnet ef migrations add <MigrationName> --output-dir Infrastructure/Data/Migrations
    dotnet ef database update
    dotnet ef migrations script --output migration.sql
    dotnet ef migrations remove
    dotnet ef database drop --force

## Conventions

1. Migration names should be descriptive.
2. Never edit already-applied migration files. Create a new migration instead.
3. Keep entity configurations in Infrastructure/Data/Configurations.
4. Prefer Fluent API configuration over DataAnnotations on domain entities.
5. Keep MySQL-specific provider configuration in Program.cs.
6. Do not commit production database credentials.

## Initial setup for a new developer

    cd backend/ShiftDynamics.API
    dotnet restore
    dotnet ef database update
    dotnet run

Swagger UI will be available at https://localhost:7249/swagger, or the HTTP/HTTPS port shown in the console.

## Database Provider

- Database: MySQL 8.x
- EF Core: 9.0.9
- Provider: Pomelo.EntityFrameworkCore.MySql 9.0.0
- Target Framework: .NET 10

The current baseline migration is InitialMySqlSchema.
