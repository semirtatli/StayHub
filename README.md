# StayHub — Hotel Marketplace Platform

> A production-grade, microservices-based hotel reservation marketplace built with .NET 10, React 19, and Azure.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?style=flat-square&logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?style=flat-square&logo=typescript)](https://www.typescriptlang.org/)
[![Tests](https://img.shields.io/badge/Tests-165_passing-brightgreen?style=flat-square)]()
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)

---

## Overview

**StayHub** is a Booking.com-style hotel marketplace where:
- **Guests** search, book, pay for, and review hotel stays
- **Hotel Owners** list properties, manage rooms/pricing/availability, and respond to reviews
- **Admins** approve hotels, manage users, view platform analytics, and handle disputes

Built as a **distributed microservices architecture** following **Domain-Driven Design (DDD)**, **CQRS**, and **Clean Architecture** patterns.

---

## Architecture

### System Overview

```
React Frontend (Vite + TypeScript + Tailwind)
        |
   YARP API Gateway (Rate Limiting, CORS, Auth, Routing, SignalR Hub)
        |
   +----+----+--------+--------+--------+----------+----------+
   |    |    |        |        |        |          |          |
Identity Hotel Booking Payment Review Notification Analytics
Service  Svc   Svc     Svc     Svc    Worker      Svc
   |    |    |        |        |        |          |
  SQL  SQL  SQL      SQL     SQL      SQL        SQL
   |    |    |        |        |     (Hangfire)    |
   +----+----+--------+--------+--------+----------+
                      |               |
               RabbitMQ (MassTransit) |
                                    Redis (Distributed Cache)
```

### Per-Service Architecture (Clean Architecture + DDD + CQRS)

```
API Layer           -> Controllers, Middleware, Swagger, Health Checks
Application Layer   -> Commands, Queries, Handlers (MediatR), Validators, Behaviors
Domain Layer        -> Entities, Value Objects, Aggregates, Domain Events, Specifications
Infrastructure Layer -> EF Core, Repositories, Migrations, External Clients, MassTransit
```

---

## Tech Stack

| Category | Technology |
|----------|-----------|
| **Backend** | .NET 10, ASP.NET Core, C# 13 |
| **Frontend** | React 19, TypeScript 5.9, Vite 7, Tailwind CSS 4 |
| **Database** | SQL Server 2022 (database-per-service) |
| **ORM** | Entity Framework Core 10 (Code-First Migrations) |
| **Messaging** | RabbitMQ + MassTransit (Outbox Pattern) |
| **API Gateway** | YARP (rate limiting, CORS, auth forwarding) |
| **Auth** | ASP.NET Core Identity + JWT (access + refresh tokens) |
| **Real-time** | SignalR (WebSocket notifications) |
| **Caching** | Redis (distributed cache via StackExchange.Redis) |
| **Background Jobs** | Hangfire (scheduled tasks, retries, dashboard) |
| **Payments** | Stripe (PaymentIntent, refunds, webhooks) |
| **CQRS/Mediator** | MediatR (pipeline behaviors for validation, logging, transactions) |
| **Validation** | FluentValidation (automatic via pipeline) |
| **Logging** | Serilog (structured) -> Seq (dev) / App Insights (prod) |
| **Containers** | Docker (multi-stage builds, non-root users) |
| **Orchestration** | Kubernetes (AKS) + Helm |
| **CI/CD** | GitHub Actions (build, test, Docker push) |
| **Testing** | xUnit, NSubstitute, FluentAssertions, Testcontainers, Vitest, Playwright |

---

## Key Features

### Production-Ready Infrastructure
- **EF Core Migrations** — proper database versioning (not `EnsureCreated`)
- **Distributed caching** with Redis for read-heavy endpoints
- **Rate limiting** with retry-after headers (fixed + sliding window per IP)
- **Correlation ID** propagation across all services for distributed tracing
- **Health checks** on every service (DB connectivity, used by Docker/K8s)
- **Outbox pattern** for reliable event publishing (solves dual-write problem)

### API Design
- **Consistent API envelope** — all responses wrapped in `ApiResponse<T>` with `success`, `data`, `errors`, `meta` fields
- **Shared `ApiControllerBase`** — DRY Result-to-HTTP mapping across all 7 services
- **API versioning** (header-based) via Asp.Versioning
- **Swagger/OpenAPI** documentation with JWT auth on every service

### Authentication & Authorization
- JWT access tokens (15min) + httpOnly cookie refresh tokens (7 days)
- Role-based auth: Guest, HotelOwner, Admin
- Password reset flow (forgot password + token-based reset)
- Account lockout after failed attempts
- Resource-based authorization (owners manage own resources only)

### Real-time & Background Processing
- **SignalR hub** for real-time notifications (booking confirmed, payment status, etc.)
- **Hangfire** for scheduled background jobs (notification cleanup, pending retries, daily digest)
- **MassTransit consumers** for async event-driven communication between services

### Frontend
- **React Error Boundary** — graceful error handling with dev stack traces
- **Skeleton loading states** for all data-heavy pages
- **Automatic JWT refresh** with failed request queue (no UX interruption)
- **Client-side validation** mirroring backend rules
- **React Query** for server state management and caching

### Testing
- **165 unit tests** across 5 services (Identity, Hotel, Booking, Payment, Review)
- **Integration tests** with Testcontainers (SQL Server + RabbitMQ)
- **Frontend tests** with Vitest + React Testing Library
- **E2E test infrastructure** with Playwright

### Database
- **Database-per-service** isolation (7 independent SQL Server databases)
- **47 database indexes** for production query performance
- **Audit trail** — automatic `CreatedBy`/`LastModifiedBy` via EF Core interceptor
- **Soft delete** with global query filters
- **Optimistic concurrency** support

---

## Project Structure

```
StayHub/
├── .github/workflows/          # CI/CD pipelines
├── deploy/
│   ├── docker/                 # Docker Compose configs
│   ├── k8s/                    # Kubernetes manifests + Helm charts
│   └── azure/                  # Bicep/ARM templates
├── docs/
│   ├── architecture/           # C4 diagrams, system overview
│   ├── adr/                    # Architecture Decision Records (8 ADRs)
│   ├── diagrams/               # Visual diagrams
│   └── database/               # DB schemas & ERDs
├── src/
│   ├── ApiGateway/             # YARP API Gateway + SignalR Hub
│   ├── Services/
│   │   ├── Identity/           # Auth, users, JWT, password reset
│   │   ├── Hotel/              # Hotel & room CRUD, search, availability
│   │   ├── Booking/            # Reservations, state machine, cancellations
│   │   ├── Payment/            # Stripe payments, refunds, webhooks
│   │   ├── Review/             # Ratings, reviews, management responses
│   │   ├── Notification/       # Email notifications, Hangfire jobs
│   │   └── Analytics/          # Revenue, occupancy, booking trends
│   ├── Shared/
│   │   ├── StayHub.Shared/             # Domain primitives, Result<T>, Guards
│   │   ├── StayHub.Shared.Infrastructure/ # BaseDbContext, Repositories, Caching, Outbox
│   │   └── StayHub.Shared.Web/         # ApiControllerBase, Middleware, SignalR, Resilience
│   └── Frontend/stayhub-web/   # React 19 SPA
├── tests/
│   ├── Services/               # Unit + Integration tests per service
│   └── FrontendTests/          # Playwright E2E tests
├── docker-compose.yml          # Local development
├── docker-compose.prod.yml     # Production overrides (resource limits, logging)
├── .editorconfig               # Code style enforcement
├── Directory.Build.props       # Shared MSBuild properties
├── Directory.Packages.props    # NuGet Central Package Management
└── StayHub.slnx                # Solution file (.NET 10)
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22 LTS](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Quick Start (Docker Compose)

```bash
# Clone the repository
git clone https://github.com/semirtatli/StayHub.git
cd StayHub

# Start all services (infrastructure + microservices + frontend)
docker compose up -d

# Frontend:     http://localhost:3001
# API Gateway:  http://localhost:5000
# RabbitMQ UI:  http://localhost:15672 (stayhub/stayhub)
# Redis:        localhost:6379
```

### Local Development

```bash
# Start infrastructure only
docker compose up -d sqlserver rabbitmq redis

# Run backend services
dotnet run --project src/Services/Identity/StayHub.Services.Identity.Api
dotnet run --project src/Services/Hotel/StayHub.Services.Hotel.Api
# ... repeat for other services

# Run frontend
cd src/Frontend/stayhub-web
npm install
npm run dev     # http://localhost:3000
```

### Production Deployment

```bash
# Use production overrides (resource limits, logging, restart policies)
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

---

## Testing

```bash
# Run all unit tests (165 tests)
dotnet test StayHub.slnx --filter "UnitTests"

# Run integration tests (requires Docker)
dotnet test StayHub.slnx --filter "IntegrationTests"

# Run frontend tests
cd src/Frontend/stayhub-web
npm test

# Run E2E tests
npx playwright test
```

---

## API Endpoints

All endpoints are accessed through the API Gateway at `http://localhost:5000`.

| Service | Base Path | Key Endpoints |
|---------|-----------|---------------|
| **Auth** | `/api/auth` | `POST /register`, `POST /login`, `POST /refresh`, `POST /forgot-password`, `POST /reset-password` |
| **Hotels** | `/api/hotels` | `GET /search`, `GET /{id}`, `POST /`, `PUT /{id}`, `GET /my` |
| **Bookings** | `/api/bookings` | `POST /`, `GET /my`, `PUT /{id}/cancel`, `PUT /{id}/check-in` |
| **Payments** | `/api/payments` | `POST /create-intent`, `POST /confirm`, `POST /{id}/refund` |
| **Reviews** | `/api/reviews` | `POST /`, `GET /hotel/{hotelId}`, `PUT /{id}`, `DELETE /{id}` |
| **Notifications** | `/api/notifications` | `GET /`, `PUT /{id}/read` |
| **Analytics** | `/api/analytics` | `GET /revenue`, `GET /occupancy`, `GET /dashboard` |
| **SignalR** | `/hubs/notifications` | WebSocket hub for real-time notifications |

Swagger UI available per service at `/swagger` (development only).

---

## Security

- JWT access tokens (15-min expiry) + rotating refresh tokens (httpOnly cookies)
- Role-based authorization (Guest, HotelOwner, Admin)
- Resource-based authorization (owners manage own resources only)
- Rate limiting with `Retry-After` headers (100 req/min general, 30 req/min auth)
- Input validation on every command (FluentValidation pipeline)
- SQL injection prevention via EF Core parameterized queries
- CORS strict origin whitelist with credentials support
- Password reset with time-limited tokens (anti-enumeration)
- Account lockout after 5 failed attempts (15-min cooldown)
- Non-root Docker containers

---

## Documentation

- [Architecture Overview](docs/architecture/) — C4 diagrams, tech justification
- [Architecture Decision Records](docs/adr/) — ADR-001 through ADR-008
- [Database Schemas](docs/database/) — ERDs and schema documentation
- Swagger UI — available per service at `/swagger` in development

---

## Author

**Semir Tatli** — Full-stack Software Developer

- GitHub: [@semirtatli](https://github.com/semirtatli)
- Email: semirtatli@outlook.com

---

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
