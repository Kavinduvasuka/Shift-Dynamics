# Shift Dynamics Backend – Coding Conventions

## Project structure (current)

```
ShiftDynamics.API/
├── Common/                 # Shared response models, exceptions
├── Controllers/            # HTTP endpoints only – thin
├── DTOs/                   # Request / response contracts (with DataAnnotations)
├── Domain/
│   └── Entities/           # Domain models + enums
├── Infrastructure/
│   └── Data/
│       ├── Configurations/ # EF Fluent API configurations
│       ├── Migrations/
│       └── ShiftDynamicsDbContext.cs
├── Middleware/             # Cross-cutting (exception handling, etc.)
├── docs/                   # Backend-specific documentation
└── Program.cs
```

## Rules

1. **Controllers stay thin** – validation, mapping, and business rules should eventually move to application services (future Application layer).
2. **DTOs carry DataAnnotations** for request validation. Domain entities do not use DataAnnotations for validation.
3. **Use the standard error model** (`ApiError` / `AppException` hierarchy) so clients always receive consistent JSON.
4. **Never commit secrets** – connection strings, JWT keys, email credentials go in User Secrets / environment variables / vault.
5. **Migrations** – never edit applied migrations; always add a new one.
6. **Timestamps** – prefer `DateTime.UtcNow` for all stored timestamps.
7. **IDs** – use `Guid` for primary keys unless a sequential business number is required (JobNo, InvoiceNo, etc.).
8. **Ownership & authorization** will be enforced in later phases; do not rely on the frontend for security.
