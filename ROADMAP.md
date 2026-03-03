# StayHub — Implementation Roadmap

> This file tracks the full implementation plan. Each phase and commit is listed with its status.
> If the AI context resets, read this file + git log to resume from the correct point.

## Status Legend

- ✅ Completed (committed and pushed)
- 🔄 In Progress
- ⬚ Not Started

---

## Phase 0: Foundation & Documentation

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 1 | `feat: initialize repository with solution structure and build configuration` | ✅ | Solution structure, .editorconfig, Directory.Build.props, Directory.Packages.props, global.json, .gitignore, README, LICENSE |
| 2 | `docs: add architecture documentation with C4 diagrams and tech justification` | ✅ | system-overview.md, c4-diagrams.md, tech-stack-justification.md |
| 3 | `docs: add architecture decision records (ADR-001 through ADR-008)` | ✅ | 8 ADRs: microservices, CQRS, DB-per-service, RabbitMQ+MassTransit, YARP, outbox, clean arch, JWT |
| 4 | `docs: add domain model with bounded contexts and entity diagrams` | ✅ | Bounded context map, ubiquitous language, entity diagrams for all 7 contexts, state machines, domain events |

## Phase 1: Shared Kernel & Infrastructure

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 5 | `feat: add shared kernel library` | ✅ | `StayHub.Shared` project: base Entity, AggregateRoot, ValueObject, DomainEvent, Result<T>, IRepository, IAuditable, ISoftDeletable, pagination, Guard, Exceptions |
| 6 | `feat: add CQRS pipeline behaviors and DI registration` | ✅ | ValidationBehavior, LoggingBehavior, TransactionBehavior, UnhandledExceptionBehavior, ICommandBase marker, SharedKernelRegistration DI extension |
| 7 | `feat: add infrastructure abstractions with EF Core interceptors` | ✅ | BaseDbContext, Repository<T>, SpecificationRepository<T>, Specification pattern, AuditableEntityInterceptor, SoftDeleteInterceptor, BaseEntityConfiguration, DateTimeProvider, InfrastructureRegistration |
| 8 | `infra: add Docker Compose for SQL Server, RabbitMQ, and Seq` | ✅ | docker-compose.yml (SQL Server 2022, RabbitMQ 3.13, Seq), docker-compose.dev.yml, .env.example, health checks, named volumes, custom network |
| 9 | `feat: add YARP API Gateway with rate limiting and CORS` | ✅ | YARP reverse proxy with 7 service routes, fixed/sliding rate limiting per IP, CORS for React frontend, correlation ID middleware, global exception handler, Serilog+Seq logging, health endpoint |

## Phase 2: Identity Service

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 10 | Identity Service scaffold | ✅ | 4-layer Clean Architecture projects, EF Core + Identity dbcontext, database design |
| 11 | User registration | ✅ | Register command/handler, FluentValidation, password policy, domain events |
| 12 | Authentication (Login/JWT) | ✅ | JWT generation, refresh token rotation, secure cookie options |
| 13 | Role management | ✅ | Guest/HotelOwner/Admin roles, role assignment, authorization policies |
| 14 | Profile management | ✅ | Update profile, change password, avatar upload |
| 15 | Email verification | ✅ | Confirmation token flow, integration event to Notification Service |

## Phase 3: Hotel Service

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 16 | Hotel Service scaffold | ✅ | 4-layer architecture, domain model (Hotel aggregate, Room entity, value objects) |
| 17 | Hotel CRUD commands | ✅ | Create/Update hotel with DDD validation, GetById/GetByOwner queries, HotelsController |
| 18 | Room management | ✅ | AddRoom, UpdateRoom, RemoveRoom commands, GetRoomsByHotel query, RoomsController |
| 19 | Photo management | ✅ | IFileStorageService abstraction, LocalFileStorageService, hotel photo gallery, upload/delete/reorder |
| 20 | Search engine | ✅ | Full-text search, dynamic filtering (Specification pattern), geo-distance, pagination |
| 21 | Availability engine | ✅ | Date-range availability checks, room inventory management |
| 22 | Hotel approval workflow | ✅ | Admin approval state machine, 6 status commands, pending approvals query, HotelApprovalController |

## Phase 4: Booking Service

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 23 | Booking Service scaffold | ✅ | 4-layer Clean Architecture, BookingEntity aggregate with full state machine, 4 value objects, 6 domain events, repository, EF Core config, API host (port 5104) |
| 24 | Reservation creation | ✅ | IHotelServiceClient anti-corruption layer, CreateBooking command/validator/handler with HTTP availability validation, HotelServiceHttpClient infrastructure, BookingsController POST endpoint |
| 25 | Booking state machine | ✅ | 5 status transition commands (Confirm, CheckIn, Complete, Cancel, MarkNoShow) with domain event dispatch, CancelBooking validator, BookingsController endpoints |
| 26 | Cancellation policies | ✅ | Configurable per hotel, refund calculation based on timing |
| 27 | Outbox pattern implementation | ✅ | Store integration events in same transaction, background publisher |
| 28 | Guest booking queries | ✅ | My bookings, booking details, PDF confirmation generation |

## Phase 5: Payment Service

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 29 | Payment Service scaffold | ✅ | Domain model, Stripe abstraction (Strategy pattern) |
| 30 | Stripe PaymentIntent | ✅ | Create payment, handle 3D Secure, webhook verification |
| 31 | Payment lifecycle | ✅ | Pending→Completed→Refunded, link to booking via integration events |
| 32 | Refund processing | ✅ | Full/partial refunds via Stripe, triggers booking state update |

## Phase 6: Review Service

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 33 | Review Service scaffold | ✅ | Domain model (Review aggregate, Rating VO with categories) |
| 34 | Review CRUD | ✅ | Create (only after completed stay), update, soft-delete |
| 35 | Rating aggregation | ✅ | Recalculate hotel average on review events, cache in read model |

## Phase 7: Notification Service

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 36 | Notification Service scaffold | ✅ | Worker service, MassTransit consumers |
| 37 | Email templates | ✅ | Razor-based: booking confirmation, cancellation, payment receipt, review reminder |
| 38 | Event consumers | ✅ | BookingConfirmed→email, PaymentCompleted→receipt, etc. |

## Phase 8: Analytics Service

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 39 | Analytics Service scaffold | ✅ | Read-optimized, event-sourced projections |
| 40 | Data projections | ✅ | Revenue, occupancy rates, booking trends, top hotels |
| 41 | Admin query endpoints | ✅ | Time-series APIs, KPI summaries, CSV/Excel export |

## Phase 9: React Frontend

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 42-51 | `feat: add React frontend with Vite, Tailwind, routing, auth, and all pages.` | ✅ | Vite 7 + React 19 + TypeScript + Tailwind v4 + React Router 7 + React Query + Axios + Recharts. Full scaffold: layouts (Main, Admin), auth (login, register, verify-email), public (home, search, detail), booking flow (dates→room→confirm), guest (my-bookings, profile, reviews), owner (hotels, form, bookings), admin (dashboard with charts, users, hotels), error pages (404, 403), UI component library (Button, Input, Card, Modal, StarRating, Skeleton) |

## Phase 10: Cross-Cutting & Observability

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 52 | Structured logging | ⬚ | Serilog→Seq (dev) / App Insights (prod), correlation IDs |
| 53 | Resilience | ⬚ | Polly retry + circuit breaker on inter-service HTTP calls |
| 54 | API documentation | ⬚ | Swagger/OpenAPI per service, API versioning |

## Phase 11: Testing

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 55 | Unit tests | ⬚ | Domain logic, command handlers (xUnit + NSubstitute + FluentAssertions) |
| 56 | Integration tests | ⬚ | API tests with WebApplicationFactory + Testcontainers |
| 57 | Frontend tests | ⬚ | Component tests (Vitest + RTL), E2E (Playwright) |

## Phase 12: Kubernetes & Deployment

| # | Commit | Status | Description |
|---|--------|--------|-------------|
| 58 | Dockerfiles | ⬚ | Multi-stage builds per service + frontend (Nginx) |
| 59 | Helm charts | ⬚ | K8s manifests: Deployments, Services, Ingress, ConfigMaps, Secrets, HPA |
| 60 | AKS infrastructure | ⬚ | Bicep templates: AKS, Azure SQL, Service Bus, Key Vault, ACR |
| 61 | GitHub Actions CI/CD | ⬚ | Build→Test→Docker push→Helm deploy, per-service pipelines |
| 62 | Production deployment | ⬚ | Final deploy, TLS, DNS, smoke tests, monitoring |

---

## Key Architecture Decisions (Quick Reference)

| Decision | Choice | ADR |
|----------|--------|-----|
| Architecture | Microservices (7 services) | ADR-001 |
| Internal pattern | Clean Architecture + DDD + CQRS | ADR-002, ADR-007 |
| Database | SQL Server, database-per-service | ADR-003 |
| Messaging | RabbitMQ + MassTransit | ADR-004 |
| API Gateway | YARP | ADR-005 |
| Reliable messaging | Outbox Pattern | ADR-006 |
| Auth | ASP.NET Core Identity + JWT (15min access + 7day refresh) | ADR-008 |
| Frontend | React 19 + Vite + TypeScript + Tailwind |  |
| Orchestration | AKS (Kubernetes) + Helm |  |
| CI/CD | GitHub Actions |  |
| Payments | Stripe (test mode) |  |

## Project Info

- **Repo**: https://github.com/semirtatli/StayHub
- **Owner**: Semir Tatlı (semirtatli@outlook.com)
- **Git config**: user.name="Semir Tatlı", user.email="semirtatli@outlook.com"
- **SDK**: .NET 10.0.103
- **Solution**: StayHub.slnx (.NET 10 XML solution format)
- **Local path**: c:\Users\Sam\Desktop\hostel reservation system

## How to Resume After Context Reset

1. Read this `ROADMAP.md` to understand the full plan and current progress
2. Run `git log --oneline` to confirm which commits are done
3. Read `docs/architecture/system-overview.md` for architecture details
4. Read `docs/database/domain-model.md` for entity/aggregate details
5. Check `Directory.Packages.props` for current NuGet package versions
6. Continue from the next ⬚ item in the roadmap above
7. After each commit, update this file: change ⬚ to ✅ and add the commit message
8. Keep commit messages short: `feat: add shared kernel library`

## Conventions

- **Commit style**: `feat:`, `docs:`, `fix:`, `refactor:`, `test:`, `chore:` prefixes
- **Branch**: all work on `main` (single developer)
- **Push after every commit**
- **Explain each commit to user**: what was done, why, interview talking points
- **Wait for user confirmation** before proceeding to next commit
