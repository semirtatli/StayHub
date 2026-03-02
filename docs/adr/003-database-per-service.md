# ADR-003: Database per Service

**Date**: 2026-03-02
**Status**: Accepted

## Context

With 7 microservices, we must decide whether services share a database or each owns its own.

## Options Considered

1. **Shared database** — All services read/write the same database with a shared schema
2. **Shared database, separate schemas** — One SQL Server instance, but each service has its own schema
3. **Database per service** — Each service has its own database, fully isolated

## Decision

**Database per service** — each microservice owns its database exclusively. In development, these are separate databases on the same SQL Server instance. In production, they can be separate Azure SQL databases.

## Rationale

- **True decoupling**: Schema changes in Hotel Service cannot break Booking Service
- **Independent scaling**: Heavy-read services (Hotel search) can have read replicas without affecting others
- **Technology flexibility**: Analytics Service could use a columnar store in the future
- **Microservice principle**: A service's database is its private implementation detail

## Consequences

### Positive
- Services evolve schemas independently
- No accidental coupling through shared tables
- Each service can optimize its schema for its access patterns
- Clear data ownership

### Negative
- Cross-service queries require API calls (no JOINs across services)
- Data duplication — Booking stores `HotelId` but can't FK to Hotel table
- Reporting across services requires event-driven data projection (Analytics Service)
- More databases to manage, backup, and monitor

## Cross-Service Data References

Services reference entities from other services by ID only:

```csharp
// In Booking Service — no FK to Hotel database
public class Booking
{
    public Guid HotelId { get; private set; }     // Just the ID, no navigation
    public Guid GuestUserId { get; private set; } // Just the ID, no navigation
}
```

When a service needs data from another service, it either:
1. **Calls the other service's API** (synchronous, for real-time needs)
2. **Listens to events** and maintains a local read projection (async, for frequently accessed data)
