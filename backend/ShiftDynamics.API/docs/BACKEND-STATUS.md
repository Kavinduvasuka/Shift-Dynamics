# Shift Dynamics Backend – Implementation Status

Branch: `integration/full-stack`

## What was implemented

### Phase 1 – Foundation
- API project structure with Common, Middleware, Services, docs
- Standard `ApiResponse` / `ApiError` envelopes
- Global exception handling middleware
- DataAnnotations validation on DTOs + consistent 400 responses
- Swagger UI with JWT bearer support
- CORS for local frontend
- Health endpoints (`/api/health`, `/api/health/ready`, `/health`)
- Connection string + JWT configuration
- Migration workflow documentation

### Phase 2 – Authentication
- Expanded `User` model (`SystemRole`, `AccountStatus`)
- BCrypt password hashing
- JWT access tokens
- `POST /api/auth/register` – customer registration
- `POST /api/auth/login` – unified login
- `GET /api/auth/me` – current user
- `POST /api/auth/forgot-password` / `reset-password`
- `POST /api/auth/logout`
- Role policies: Customer, ServiceAdvisor, Manager, Mechanic, Storekeeper, Vendor, Staff

### Phase 3 – Customer & Vehicle
- Existing Customers / Vehicles controllers retained
- Linked to User via `CustomerId` on registration

### Phase 4 – Booking & Catalog
- Appointments controller (existing)
- Services catalog CRUD (`/api/services`)

### Phase 5 – Work Orders / Advisor
- Work orders API with status transitions
- Server-generated work order numbers

### Phase 6 – Estimates & Billing
- Estimates create / send / customer decision
- Invoices create / manager approve
- Payments with balance updates (transactional)

### Phase 7 – Manager
- Dashboard summary
- Workshop bays
- Job assignment with conflict checks (mechanic + bay)
- Mechanics workload list

### Phase 8 – Mechanic
- Assigned jobs list
- Labor timer start/end
- Part requisitions

### Phase 9 – Inventory & Storekeeper
- Parts + inventory list / low-stock
- Requisition queue, approve/reject, stock release (transactional, prevents negative stock)
- Stock movement ledger

### Phase 10 – Vendor
- Vendor self-registration
- Manager pending list + approve/reject (creates User on approve)

### Phase 11 – Public services
- Contact inquiries (public POST + manager inbox)
- Emergency service providers (geo radius query)
- Emergency requests (public allowed)
- Seed data for providers, services, bays, parts, demo staff

### Phase 12 – Integration notes
- Frontend still uses demo data – replace module-by-module after verifying each API
- Run `POST /api/seed` in Development to load demo staff (password `Test@1234`)

## Required before first run

```bash
cd backend/ShiftDynamics.API
dotnet restore
dotnet ef migrations add FullWorkflowExpansion --output-dir Infrastructure/Data/Migrations
dotnet ef database update
dotnet run
```

Swagger: `https://localhost:7249/swagger`

## Demo staff (after seed)

| Email | Role | Password |
|-------|------|----------|
| manager@shiftdynamics.lk | Manager | Test@1234 |
| advisor@shiftdynamics.lk | ServiceAdvisor | Test@1234 |
| mechanic@shiftdynamics.lk | Mechanic | Test@1234 |
| store@shiftdynamics.lk | Storekeeper | Test@1234 |

Register customers via `POST /api/auth/register`.

## Still incomplete / future work
- Refresh tokens
- Email provider for password reset / vendor notifications
- Full ownership enforcement on every customer endpoint
- Job status history entity & strict transition matrix
- Vendor quotes / purchase orders
- Payment gateway webhooks
- SignalR realtime
- Automated unit/integration tests
- CI/CD pipeline
