# ADR-007: Clean Architecture per Service

**Date**: 2026-03-02
**Status**: Accepted

## Context

Each microservice needs an internal project structure. We need to decide how to organize layers, manage dependencies, and keep business logic isolated from framework concerns.

## Options Considered

1. **Simple 2-layer** — API + Data (common in tutorials)
2. **N-layer** — API → Business → Data (traditional enterprise)
3. **Clean Architecture** — API → Application → Domain ← Infrastructure
4. **Vertical Slice** — Feature folders containing all layers per feature

## Decision

Use **Clean Architecture** with 4 projects per service: API, Application, Domain, Infrastructure.

## Rationale

- **Dependency Inversion**: Domain layer has ZERO external dependencies — it defines interfaces that Infrastructure implements
- **Testability**: Domain and Application layers are testable without database, HTTP, or message broker
- **Framework independence**: Switching from EF Core to Dapper changes only Infrastructure
- **DDD alignment**: Domain layer is where aggregates, value objects, and business rules live — nothing else pollutes it
- **Industry recognition**: Clean Architecture is a well-known pattern that interviewers and teams understand

## Layer Dependencies

```
API → Application → Domain ← Infrastructure
                      ↑
              Infrastructure implements
              Domain's interfaces
```

The arrow `←` is the key insight: Infrastructure depends on Domain (to implement its interfaces), NOT the other way around. Domain never knows about SQL Server, RabbitMQ, or any framework.

## Project Structure per Service

```
StayHub.{Service}.Domain/
  ├── Entities/
  ├── ValueObjects/
  ├── Aggregates/
  ├── Events/
  ├── Interfaces/          ← Repository interfaces defined here
  ├── Specifications/
  ├── Exceptions/
  └── Enums/

StayHub.{Service}.Application/
  ├── Commands/            ← Write operations
  │   └── CreateBooking/
  │       ├── CreateBookingCommand.cs
  │       ├── CreateBookingCommandHandler.cs
  │       └── CreateBookingCommandValidator.cs
  ├── Queries/             ← Read operations
  │   └── GetBookings/
  │       ├── GetBookingsQuery.cs
  │       ├── GetBookingsQueryHandler.cs
  │       └── BookingResponse.cs
  ├── Behaviors/           ← MediatR pipeline behaviors
  ├── Mappings/            ← DTO mapping profiles
  └── Interfaces/          ← Application-specific interfaces

StayHub.{Service}.Infrastructure/
  ├── Persistence/
  │   ├── {Service}DbContext.cs
  │   ├── Configurations/  ← EF Core entity type configs
  │   ├── Repositories/    ← Implements Domain interfaces
  │   ├── Migrations/
  │   └── Interceptors/
  ├── Messaging/           ← MassTransit consumers
  └── Services/            ← External API clients

StayHub.{Service}.API/
  ├── Controllers/
  ├── Middleware/
  ├── Filters/
  ├── Program.cs           ← Composition root (DI registration)
  └── appsettings.json
```

## Consequences

### Positive
- Business logic never depends on infrastructure
- Swapping infrastructure components requires zero domain changes
- Unit tests cover domain logic without any mocking of infrastructure
- Clear separation enables parallel development

### Negative
- More projects per service (4 × 7 = 28 projects + shared + gateway + frontend)
- Some boilerplate for simple CRUD operations
- Developers must understand the dependency rule
