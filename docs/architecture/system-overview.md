# StayHub — Architecture Overview

## Table of Contents

- [1. Introduction](#1-introduction)
- [2. System Context](#2-system-context)
- [3. Architecture Style](#3-architecture-style)
- [4. Service Decomposition](#4-service-decomposition)
- [5. Communication Patterns](#5-communication-patterns)
- [6. Data Architecture](#6-data-architecture)
- [7. Security Architecture](#7-security-architecture)
- [8. Deployment Architecture](#8-deployment-architecture)
- [9. Cross-Cutting Concerns](#9-cross-cutting-concerns)
- [10. Technology Stack](#10-technology-stack)

---

## 1. Introduction

**StayHub** is a production-grade hotel marketplace platform (Online Travel Agency — OTA) that connects hotel owners with guests. Think of it as a Booking.com-style system where:

- **Guests** discover hotels, make reservations, process payments, and leave reviews
- **Hotel Owners** list properties, manage room inventory and pricing, handle bookings
- **Platform Admins** approve hotel listings, manage users, monitor platform health and revenue

### Business Goals

| Goal | How StayHub Addresses It |
|------|--------------------------|
| **Scalability** | Microservices scale independently — Booking Service scales during peak season without scaling Review Service |
| **Reliability** | Service isolation — Payment Service failure doesn't prevent hotel browsing |
| **Time-to-Market** | Independent deployment — Hotel Service team deploys without coordinating with Booking team |
| **Multi-tenancy** | Platform supports unlimited hotel owners, each managing their own properties |

### Quality Attributes (Non-Functional Requirements)

| Attribute | Target |
|-----------|--------|
| **Availability** | 99.9% uptime for booking and payment flows |
| **Latency** | < 200ms for search results, < 500ms for booking creation |
| **Throughput** | Support 1000+ concurrent users |
| **Security** | OWASP Top 10 compliance, encrypted data at rest and in transit |
| **Maintainability** | Each service independently deployable in < 15 minutes |

---

## 2. System Context

The **C4 Model Level 1 — System Context Diagram** shows StayHub and its external interactions.

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                           SYSTEM CONTEXT                                     │
│                                                                              │
│   ┌─────────┐         ┌───────────────────────┐         ┌─────────┐         │
│   │  Guest  │────────▶│                       │◀────────│  Hotel  │         │
│   │  (User) │ Browse, │      StayHub          │ Manage  │  Owner  │         │
│   │         │ Book,   │  Hotel Marketplace    │ Hotels, │  (User) │         │
│   │         │ Pay,    │      Platform         │ Rooms,  │         │         │
│   │         │ Review  │                       │ Pricing │         │         │
│   └─────────┘         └───────────┬───────────┘         └─────────┘         │
│                              │    │    │                                      │
│        ┌─────────┐           │    │    │           ┌─────────────┐           │
│        │  Admin  │───────────┘    │    └──────────▶│   Stripe    │           │
│        │  (User) │  Approve,      │    Payments    │  (External) │           │
│        │         │  Manage,       │                └─────────────┘           │
│        └─────────┘  Analytics     │                                          │
│                                   │                ┌─────────────┐           │
│                                   └───────────────▶│   SendGrid  │           │
│                                        Emails      │  (External) │           │
│                                                    └─────────────┘           │
└──────────────────────────────────────────────────────────────────────────────┘
```

### External Systems

| System | Integration | Purpose |
|--------|-------------|---------|
| **Stripe** | REST API + Webhooks | Payment processing, refunds, 3D Secure |
| **SendGrid/SMTP** | SMTP / REST API | Transactional emails (booking confirmations, receipts) |
| **Azure Blob Storage** | Azure SDK | Hotel and room photo storage |
| **Azure Key Vault** | Azure SDK | Production secrets management |

---

## 3. Architecture Style

### Microservices Architecture

StayHub uses a **microservices architecture** where each business domain is an independent, deployable service with its own database.

**Why microservices over a monolith?**

| Concern | Monolith | Microservices (StayHub) |
|---------|----------|------------------------|
| **Deployment** | Deploy everything for any change | Deploy only the changed service |
| **Scaling** | Scale everything equally | Scale hot services (Booking) independently |
| **Technology** | One stack for everything | Each service can optimize independently |
| **Failure isolation** | One bug crashes everything | Payment failure doesn't affect search |
| **Team autonomy** | Everyone works in one codebase | Teams own services end-to-end |
| **Complexity** | Simple deployment, complex code | Complex deployment, simpler code per service |

**Trade-offs acknowledged:**

- **Network latency**: Inter-service calls add latency vs. in-process calls
- **Distributed transactions**: No simple `BEGIN TRANSACTION` across services — we use the Outbox pattern + eventual consistency
- **Operational complexity**: More services = more monitoring, logging, and deployment pipelines
- **Data consistency**: Eventual consistency between services requires careful domain design

### Internal Service Architecture: Clean Architecture + DDD + CQRS

Each microservice follows **Clean Architecture** (Robert C. Martin) with **Domain-Driven Design** and **CQRS**:

```
┌─────────────────────────────────────────────────────────────────┐
│                        API Layer                                │
│  ASP.NET Core Controllers · Middleware · Exception Filters      │
│  Request/Response DTOs · Swagger Documentation                  │
│                                                                 │
│  Depends on: Application Layer                                  │
│  Responsibility: HTTP concerns only — routing, serialization,   │
│                  authentication, API versioning                  │
├─────────────────────────────────────────────────────────────────┤
│                    Application Layer                             │
│  Commands (Write) · Queries (Read) · MediatR Handlers           │
│  FluentValidation Validators · Pipeline Behaviors               │
│  DTO Mappings · Integration Event Publishing                    │
│                                                                 │
│  Depends on: Domain Layer                                       │
│  Responsibility: Orchestrate use cases. NO business rules here. │
│                  Thin layer that coordinates domain operations.  │
├─────────────────────────────────────────────────────────────────┤
│                      Domain Layer                                │
│  Entities · Aggregate Roots · Value Objects · Domain Events      │
│  Repository Interfaces · Domain Services · Specifications       │
│  Enumerations · Exceptions · Business Rules                     │
│                                                                 │
│  Depends on: NOTHING (zero external dependencies)               │
│  Responsibility: ALL business logic lives here.                 │
│                  The heart of the application.                   │
├─────────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                           │
│  EF Core DbContext · Repository Implementations · Migrations    │
│  Entity Type Configurations · Interceptors (Audit, Soft Delete) │
│  MassTransit Consumers · External API Clients · File Storage    │
│                                                                 │
│  Depends on: Domain Layer (implements its interfaces)           │
│  Responsibility: All I/O — database, message bus, external APIs │
└─────────────────────────────────────────────────────────────────┘
```

**Dependency Rule**: Dependencies point INWARD only. The Domain Layer never references Infrastructure, Application, or API layers. This means:
- Business logic is testable without a database
- You can swap SQL Server for PostgreSQL by changing only Infrastructure
- API layer changes (REST → gRPC) don't touch business logic

---

## 4. Service Decomposition

Services are split along **bounded contexts** (DDD term) — each service owns a specific business capability.

```
┌──────────────────────────────────────────────────────────────────────────┐
│                        BOUNDED CONTEXTS                                  │
│                                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                   │
│  │   Identity   │  │    Hotel     │  │   Booking    │                   │
│  │   Context    │  │   Context    │  │   Context    │                   │
│  │              │  │              │  │              │                   │
│  │ • Users      │  │ • Hotels     │  │ • Bookings   │                   │
│  │ • Roles      │  │ • Rooms      │  │ • DateRanges │                   │
│  │ • Auth       │  │ • Amenities  │  │ • Guests     │                   │
│  │ • Profiles   │  │ • Photos     │  │ • Policies   │                   │
│  │ • Tokens     │  │ • Pricing    │  │ • Inventory  │                   │
│  └──────────────┘  │ • Locations  │  └──────────────┘                   │
│                    │ • Approvals  │                                      │
│  ┌──────────────┐  └──────────────┘  ┌──────────────┐                   │
│  │   Payment    │                    │    Review    │                   │
│  │   Context    │  ┌──────────────┐  │   Context    │                   │
│  │              │  │ Notification │  │              │                   │
│  │ • Payments   │  │   Context    │  │ • Reviews    │                   │
│  │ • Refunds    │  │              │  │ • Ratings    │                   │
│  │ • Invoices   │  │ • Templates  │  │ • Responses  │                   │
│  │ • Stripe     │  │ • Emails     │  │ • Aggregates │                   │
│  └──────────────┘  │ • Events     │  └──────────────┘                   │
│                    └──────────────┘                                      │
│  ┌──────────────┐                                                       │
│  │  Analytics   │                                                       │
│  │   Context    │                                                       │
│  │              │                                                       │
│  │ • Revenue    │                                                       │
│  │ • Occupancy  │                                                       │
│  │ • Trends     │                                                       │
│  │ • KPIs       │                                                       │
│  └──────────────┘                                                       │
└──────────────────────────────────────────────────────────────────────────┘
```

### Service Responsibilities

| Service | Aggregate Root | Key Operations | Port |
|---------|---------------|----------------|------|
| **Identity** | `User` | Register, Login, JWT/Refresh, Roles, Profile | 5101 |
| **Hotel** | `Hotel` | CRUD Hotels/Rooms, Search, Availability, Photos, Approval | 5102 |
| **Booking** | `Booking` | Create Reservation, State Transitions, Cancellation, PDF | 5103 |
| **Payment** | `Payment` | Stripe PaymentIntent, Webhooks, Refunds | 5104 |
| **Review** | `Review` | Create/Update Review, Rating Aggregation | 5105 |
| **Notification** | `Notification` | Email Templates, Event Consumers, Delivery Tracking | 5106 |
| **Analytics** | — (read-only) | Revenue, Occupancy, Trends, Exports | 5107 |
| **API Gateway** | — | Routing, Rate Limiting, Auth, CORS | 5000 |

---

## 5. Communication Patterns

### Synchronous (HTTP)

Used when the caller **needs an immediate response**:

```
Guest → API Gateway → Hotel Service: "Search hotels in Istanbul"
                    → Hotel Service: "Get room availability for dates"
Booking Service → Hotel Service: "Validate room is still available" (before creating booking)
```

### Asynchronous (RabbitMQ via MassTransit)

Used for **fire-and-forget** operations and **cross-service coordination**:

```
Booking Service ──publishes──▶ BookingConfirmedEvent
                                    │
                    ┌───────────────┼───────────────┐
                    ▼               ▼               ▼
             Payment Service  Notification Svc  Analytics Svc
             "Create payment" "Send confirm    "Record booking
              for booking"     email"           for analytics"
```

### Key Integration Events

| Event | Publisher | Consumers | Purpose |
|-------|-----------|-----------|---------|
| `UserRegisteredEvent` | Identity | Notification | Welcome email |
| `HotelApprovedEvent` | Hotel | Notification | Notify owner of approval |
| `BookingCreatedEvent` | Booking | Payment, Notification | Initiate payment, send pending email |
| `BookingConfirmedEvent` | Booking | Notification, Analytics | Confirmation email, record analytics |
| `BookingCancelledEvent` | Booking | Payment, Notification, Analytics | Process refund, send cancellation email |
| `PaymentCompletedEvent` | Payment | Booking, Notification | Confirm booking, send receipt |
| `PaymentFailedEvent` | Payment | Booking, Notification | Cancel booking, notify guest |
| `ReviewCreatedEvent` | Review | Hotel, Notification, Analytics | Update hotel rating, notify owner |

### Outbox Pattern (Guaranteed Delivery)

To prevent message loss when a service saves data but crashes before publishing the event:

```
1. BEGIN TRANSACTION
2.   INSERT INTO Bookings (...)
3.   INSERT INTO OutboxMessages (EventType, Payload, CreatedAt, ProcessedAt=NULL)
4. COMMIT TRANSACTION

5. Background job polls OutboxMessages WHERE ProcessedAt IS NULL
6. Publishes to RabbitMQ
7. Marks ProcessedAt = DateTime.UtcNow
```

This guarantees **at-least-once delivery** — if step 6 fails, the background job retries.

---

## 6. Data Architecture

### Database-per-Service

Each service owns its database exclusively. No service directly queries another service's database.

```
Identity Service ──▶ StayHub_Identity_DB
Hotel Service    ──▶ StayHub_Hotel_DB
Booking Service  ──▶ StayHub_Booking_DB
Payment Service  ──▶ StayHub_Payment_DB
Review Service   ──▶ StayHub_Review_DB
Notification Svc ──▶ StayHub_Notification_DB
Analytics Svc    ──▶ StayHub_Analytics_DB
```

**Why database-per-service?**

| Shared DB | Database-per-Service |
|-----------|---------------------|
| Schema changes affect multiple services | Each service evolves its schema independently |
| Tight coupling via foreign keys across domains | Loose coupling — cross-reference by ID only |
| Single point of failure | One DB failure doesn't cascade |
| Can't scale DB per service | Each DB can be scaled independently |

### Cross-Service References

Services reference entities from other services **by ID only**, never by foreign key:

```csharp
// In Booking Service — references a Hotel, but doesn't FK to Hotel DB
public class Booking : AggregateRoot
{
    public Guid HotelId { get; private set; }     // ← Just the ID
    public Guid RoomId { get; private set; }      // ← Just the ID
    public Guid GuestUserId { get; private set; } // ← Just the ID
    // NO navigation properties to Hotel or User entities
}
```

### Standard Table Columns (All Services)

Every table includes:

| Column | Type | Purpose |
|--------|------|---------|
| `Id` | `uniqueidentifier` (GUID) | Primary key — GUIDs enable distributed ID generation |
| `CreatedAt` | `datetime2` | When the record was created (UTC) |
| `CreatedBy` | `nvarchar(256)` | Who created it (user ID or system) |
| `LastModifiedAt` | `datetime2` | When last updated (UTC) |
| `LastModifiedBy` | `nvarchar(256)` | Who updated it |
| `IsDeleted` | `bit` | Soft delete flag — records are never physically deleted |
| `DeletedAt` | `datetime2` | When soft-deleted |
| `DeletedBy` | `nvarchar(256)` | Who deleted it |
| `RowVersion` | `rowversion` | Optimistic concurrency token |

### CQRS Read/Write Split

**Commands (Writes)** go through the full domain model:
```
Controller → MediatR → Command Handler → Domain Entity → Repository → Database
```

**Queries (Reads)** bypass the domain for performance:
```
Controller → MediatR → Query Handler → DbContext/Dapper → Database → Read DTO
```

This means read operations can use optimized queries, projections, and even separate read-optimized views without polluting the domain model.

---

## 7. Security Architecture

### Authentication Flow

```
┌────────┐     ┌───────────┐     ┌──────────────┐
│ Client │────▶│ API       │────▶│ Identity     │
│ (React)│     │ Gateway   │     │ Service      │
│        │◀────│           │◀────│              │
│        │ JWT │           │ JWT │ • Validates  │
│        │     │ • Forward │     │   credentials│
└────────┘     │   JWT to  │     │ • Issues JWT │
               │   services│     │ • Refresh    │
               └───────────┘     │   tokens     │
                                 └──────────────┘
```

1. Client sends credentials to `/api/identity/login` (via Gateway)
2. Identity Service validates, returns JWT access token (15 min) + refresh token (7 days)
3. Client stores JWT in memory, refresh token in httpOnly cookie
4. Subsequent requests include JWT in `Authorization: Bearer <token>` header
5. API Gateway forwards JWT to downstream services
6. Each service validates JWT signature independently (shared signing key)
7. When JWT expires, client uses refresh token to get a new pair

### Authorization Levels

| Level | Implementation | Example |
|-------|---------------|---------|
| **Role-based** | `[Authorize(Roles = "Admin")]` | Only admins can approve hotels |
| **Policy-based** | Custom `IAuthorizationHandler` | Hotel owners can only edit their own hotels |
| **Resource-based** | Check ownership in handler | Guest can only cancel their own booking |

---

## 8. Deployment Architecture

### Development Environment (Docker Compose)

```
┌─────────────────────────────────────────────────────────┐
│                   Docker Compose                         │
│                                                          │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐              │
│  │ Identity │  │  Hotel   │  │ Booking  │  ...more      │
│  │  :5101   │  │  :5102   │  │  :5103   │  services     │
│  └──────────┘  └──────────┘  └──────────┘              │
│                                                          │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐              │
│  │ SQL      │  │ RabbitMQ │  │   Seq    │              │
│  │ Server   │  │  :5672   │  │  :5341   │              │
│  │  :1433   │  │  :15672  │  │ (Logging)│              │
│  └──────────┘  └──────────┘  └──────────┘              │
│                                                          │
│  ┌──────────┐  ┌──────────┐                             │
│  │  YARP    │  │  React   │                             │
│  │ Gateway  │  │  :3000   │                             │
│  │  :5000   │  │          │                             │
│  └──────────┘  └──────────┘                             │
└─────────────────────────────────────────────────────────┘
```

### Production Environment (Azure Kubernetes Service)

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Azure Kubernetes Service (AKS)                    │
│                                                                      │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │                    Ingress Controller (Nginx)                  │  │
│  │                    TLS Termination · Routing                   │  │
│  └────────────────────────────┬───────────────────────────────────┘  │
│                               │                                      │
│  ┌────────────────────────────▼───────────────────────────────────┐  │
│  │              YARP API Gateway (ClusterIP Service)              │  │
│  │              HPA: 2-5 replicas                                │  │
│  └────────────────────────────┬───────────────────────────────────┘  │
│                               │                                      │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐  │
│  │Identity  │ │ Hotel    │ │Booking   │ │Payment   │ │Review    │  │
│  │ Pod(s)   │ │ Pod(s)   │ │ Pod(s)   │ │ Pod(s)   │ │ Pod(s)   │  │
│  │ HPA:1-3  │ │ HPA:2-5  │ │ HPA:2-10 │ │ HPA:1-3  │ │ HPA:1-2  │  │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘  │
│                                                                      │
│  ┌──────────┐ ┌──────────┐                                          │
│  │Notif.    │ │Analytics │                                          │
│  │Worker(s) │ │ Pod(s)   │                                          │
│  │ HPA:1-2  │ │ HPA:1-2  │                                          │
│  └──────────┘ └──────────┘                                          │
└─────────────────────────────────────────────────────────────────────┘

External Azure Services:
┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│  Azure SQL   │ │ Azure Blob   │ │  Azure Key   │ │   Azure      │
│  Databases   │ │   Storage    │ │    Vault     │ │  Container   │
│ (per service)│ │  (Photos)    │ │  (Secrets)   │ │  Registry    │
└──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘
```

### Scaling Strategy

| Service | Expected Load | Min/Max Pods | Scale Trigger |
|---------|--------------|--------------|---------------|
| API Gateway | All traffic | 2 / 5 | CPU > 70% |
| Hotel Service | High (search) | 2 / 5 | CPU > 70% |
| Booking Service | Spiky (peak hours) | 2 / 10 | CPU > 60%, Request queue |
| Payment Service | Moderate | 1 / 3 | CPU > 70% |
| Identity Service | At login spikes | 1 / 3 | CPU > 70% |
| Review Service | Low | 1 / 2 | CPU > 80% |
| Notification Worker | Async, bursty | 1 / 2 | Queue depth |
| Analytics Service | Low, admin only | 1 / 2 | CPU > 80% |

---

## 9. Cross-Cutting Concerns

### Logging & Observability

```
All Services ──▶ Serilog (structured logging)
                    │
                    ├──▶ Console (development)
                    ├──▶ Seq (development — log aggregation/search)
                    └──▶ Azure Application Insights (production)

Correlation:
  Request enters Gateway → CorrelationId generated → Propagated to all downstream services
  Every log entry includes: CorrelationId, ServiceName, UserId, RequestPath, Timestamp
```

### Resilience

| Pattern | Implementation | Purpose |
|---------|---------------|---------|
| **Retry** | Polly (3 retries, exponential backoff) | Transient failures (network blips) |
| **Circuit Breaker** | Polly (break after 5 failures, wait 30s) | Prevent cascading failures |
| **Timeout** | HttpClient timeout (10s default) | Don't wait forever for a dead service |
| **Bulkhead** | Separate HttpClient instances per service | One slow service doesn't exhaust connection pool |

### Health Checks

Every service exposes:
- `GET /health/live` — "Is the process running?" (Kubernetes liveness probe)
- `GET /health/ready` — "Can it serve requests?" (checks DB, RabbitMQ connectivity)

API Gateway aggregates all service health checks at `GET /health`.

---

## 10. Technology Stack

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Runtime** | .NET | 10.0 | Latest LTS (Nov 2025), top performance, native AOT support |
| **Language** | C# | 13 | Primary constructors, collection expressions, pattern matching |
| **Web Framework** | ASP.NET Core | 10.0 | Fastest mainstream web framework (TechEmpower benchmarks) |
| **ORM** | Entity Framework Core | 10.0 | Code-first, LINQ, migrations, interceptors, Microsoft-supported |
| **Database** | SQL Server | 2022 | Enterprise reliability, Azure SQL compatibility, full-text search |
| **Messaging** | RabbitMQ | 3.13+ | Industry standard AMQP broker, lightweight, battle-tested |
| **Message Abstraction** | MassTransit | 8.x | Sagas, retries, dead-letter, outbox — don't reinvent the wheel |
| **API Gateway** | YARP | 2.x | Microsoft-built, high performance, flexible reverse proxy |
| **Mediator / CQRS** | MediatR | 12.x | Clean pipeline behaviors, decouples controllers from handlers |
| **Validation** | FluentValidation | 11.x | Rich validation rules, MediatR pipeline integration |
| **Auth** | ASP.NET Core Identity | 10.0 | User management, password hashing, role system built-in |
| **JWT** | System.IdentityModel.Tokens.Jwt | - | Standard JWT creation and validation |
| **Logging** | Serilog | 4.x | Structured logging, rich sinks ecosystem, request logging |
| **Log Aggregation** | Seq (dev) / App Insights (prod) | - | Powerful query language, dashboards |
| **Payments** | Stripe.net | 47.x | Stripe SDK — PaymentIntents, webhooks, test mode |
| **Frontend** | React | 19 | Component model, massive ecosystem, hooks |
| **Build Tool** | Vite | 6.x | Fast HMR, native ESM, modern default for React |
| **Type System** | TypeScript | 5.x | Compile-time type safety for frontend |
| **CSS** | Tailwind CSS | 4.x | Utility-first, no CSS-in-JS overhead, responsive |
| **State/Data** | React Query (TanStack) | 5.x | Server state management, caching, background refetching |
| **i18n** | react-i18next | - | Industry standard React localization |
| **Containers** | Docker | - | Consistent environments, multi-stage builds |
| **Orchestration** | Kubernetes (AKS) | - | Auto-scaling, self-healing, rolling deployments |
| **Package Manager** | Helm | 3.x | Kubernetes package manager, templated manifests |
| **CI/CD** | GitHub Actions | - | Native GitHub integration, matrix builds |
| **IaC** | Bicep | - | Azure-native, simpler than ARM templates |
| **Unit Tests** | xUnit | - | Most popular .NET test framework |
| **Mocking** | NSubstitute | - | Clean syntax, no lambda setup boilerplate |
| **Assertions** | FluentAssertions | - | Readable assertions: `result.Should().BeEquivalentTo(expected)` |
| **Integration Tests** | Testcontainers | - | Real SQL Server + RabbitMQ in Docker during tests |
| **E2E Tests** | Playwright | - | Cross-browser, auto-wait, Microsoft-backed |

---

*This document is the living architecture reference for StayHub. It will be updated as the system evolves.*
