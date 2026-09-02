# Shift Dynamics – Database Migration Workflow

## Prerequisites

- .NET 10 SDK
- PostgreSQL running locally (or reachable via connection string)
- Connection string configured via:
  - `appsettings.Development.json` (local only – do not commit real secrets), or
  - User Secrets: `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;..."` , or
  - Environment variable: `ConnectionStrings__DefaultConnection`

## Common commands (run from `backend/ShiftDynamics.API`)

```bash
# Add a new migration after changing entities / configurations
dotnet ef migrations add <MigrationName> --output-dir Infrastructure/Data/Migrations

# Apply all pending migrations to the database
dotnet ef database update

# Generate SQL script (useful for staging/production review)
dotnet ef migrations script --output migration.sql

# Remove the last migration (only if not applied)
dotnet ef migrations remove

# Drop the database (development only)
dotnet ef database drop --force
```

## Conventions

1. Migration names should be descriptive: `AddJobCards`, `AddInventoryItems`, etc.
2. Never edit already-applied migration files. Create a new migration instead.
3. Keep entity configurations in `Infrastructure/Data/Configurations`.
4. Prefer Fluent API configuration over DataAnnotations on domain entities.

## Initial setup for a new developer

```bash
cd backend/ShiftDynamics.API
dotnet restore
dotnet ef database update
dotnet run
```

Swagger UI will be available at: `https://localhost:7249/swagger` (or the http port shown in the console).
