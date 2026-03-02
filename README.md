# 🏨 StayHub — Hotel Marketplace Platform

> A production-grade, microservices-based hotel reservation marketplace (OTA) built with .NET 10, React, and Azure.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?style=flat-square&logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?style=flat-square&logo=typescript)](https://www.typescriptlang.org/)
[![Azure](https://img.shields.io/badge/Azure-AKS-0078D4?style=flat-square&logo=microsoftazure)](https://azure.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)

---

## 📋 Overview

**StayHub** is a Booking.com-style hotel marketplace where:
- **Guests** search, book, pay for, and review hotel stays
- **Hotel Owners** list properties, manage rooms/pricing/availability, and respond to reviews
- **Admins** approve hotels, manage users, view platform analytics, and handle disputes

Built as a **distributed microservices architecture** following **Domain-Driven Design (DDD)** and **CQRS** patterns, deployed to **Azure Kubernetes Service (AKS)**.

---

## 🏗️ Architecture

### System Overview

```
React Frontend (Vite + TypeScript + Tailwind)
        │
   YARP API Gateway (Rate Limiting, Auth, Routing)
        │
   ┌────┼────┬────────┬────────┬────────┬──────────┬──────────┐
   │    │    │        │        │        │          │          │
Identity Hotel Booking Payment Review Notification Analytics
Service  Svc   Svc     Svc     Svc    Worker      Svc
   │    │    │        │        │        │          │
  SQL  SQL  SQL      SQL     SQL      SQL        SQL
   │    │    │        │        │        │          │
   └────┴────┴────────┴────────┴────────┴──────────┘
                      │
               RabbitMQ (MassTransit)
```

### Per-Service Architecture (Clean Architecture + DDD + CQRS)

```
API Layer           → Controllers, Middleware, Filters, DTOs
Application Layer   → Commands, Queries, Handlers (MediatR), Validators, Behaviors
Domain Layer        → Entities, Value Objects, Aggregates, Domain Events, Specifications
Infrastructure Layer → EF Core, Repositories, Migrations, External Clients, MassTransit
```

---

## 🛠️ Tech Stack

| Category | Technology |
|----------|-----------|
| **Backend** | .NET 10, ASP.NET Core, C# 13 |
| **Frontend** | React 19, TypeScript, Vite, Tailwind CSS |
| **Database** | SQL Server (database-per-service) |
| **ORM** | Entity Framework Core 10 (Code-First) |
| **Messaging** | RabbitMQ + MassTransit |
| **API Gateway** | YARP (Yet Another Reverse Proxy) |
| **Auth** | ASP.NET Core Identity + JWT |
| **Payments** | Stripe (test mode) |
| **CQRS/Mediator** | MediatR |
| **Validation** | FluentValidation |
| **Logging** | Serilog → Seq (dev) / App Insights (prod) |
| **Containers** | Docker + Docker Compose |
| **Orchestration** | Kubernetes (AKS) + Helm |
| **CI/CD** | GitHub Actions |
| **Cloud** | Microsoft Azure |
| **Testing** | xUnit, NSubstitute, FluentAssertions, Testcontainers, Playwright |

---

## 📂 Project Structure

```
StayHub/
├── .github/workflows/          # CI/CD pipelines
├── deploy/
│   ├── docker/                 # Docker Compose & Dockerfiles
│   ├── k8s/helm/               # Helm charts for AKS
│   └── azure/                  # Bicep/ARM templates
├── docs/
│   ├── architecture/           # C4 diagrams, system overview
│   ├── adr/                    # Architecture Decision Records
│   ├── diagrams/               # Visual diagrams
│   └── database/               # DB schemas & ERDs
├── src/
│   ├── ApiGateway/             # YARP API Gateway
│   ├── Services/
│   │   ├── Identity/           # User management & authentication
│   │   ├── Hotel/              # Hotel & room management
│   │   ├── Booking/            # Reservation management
│   │   ├── Payment/            # Stripe payment processing
│   │   ├── Review/             # Ratings & reviews
│   │   ├── Notification/       # Email notifications (worker)
│   │   └── Analytics/          # Reporting & analytics
│   ├── Shared/                 # Shared kernel library
│   └── Frontend/               # React application
├── tests/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   └── FrontendTests/
├── .editorconfig               # Code style enforcement
├── .gitignore
├── Directory.Build.props       # Shared MSBuild properties
├── Directory.Packages.props    # NuGet Central Package Management
├── global.json                 # .NET SDK version pinning
├── LICENSE
├── README.md
└── StayHub.slnx                # Solution file (.NET 10 XML format)
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22 LTS](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

### Quick Start (Docker Compose)

```bash
# Clone the repository
git clone https://github.com/semirtatli/StayHub.git
cd StayHub

# Start all services
docker compose -f deploy/docker/docker-compose.yml up -d

# Frontend runs at: http://localhost:3000
# API Gateway at:   http://localhost:5000
# Seq Logging at:   http://localhost:5341
# RabbitMQ UI at:   http://localhost:15672
```

### Local Development

```bash
# Restore packages
dotnet restore

# Run a specific service
cd src/Services/Identity/StayHub.Identity.API
dotnet run

# Run frontend
cd src/Frontend
npm install
npm run dev
```

---

## 🧪 Testing

```bash
# Run all unit tests
dotnet test tests/UnitTests/

# Run integration tests (requires Docker)
dotnet test tests/IntegrationTests/

# Run frontend tests
cd src/Frontend
npm test

# Run E2E tests
npx playwright test
```

---

## 📦 Deployment

StayHub deploys to **Azure Kubernetes Service (AKS)** via GitHub Actions:

1. **Build & Test** → Compile, run tests
2. **Docker Build** → Multi-stage builds, push to Azure Container Registry
3. **Helm Deploy** → Rolling deployment to AKS cluster

See [deploy/](deploy/) for infrastructure-as-code (Bicep) and Helm charts.

---

## 🔒 Security

- JWT access tokens (15-min expiry) + rotating refresh tokens
- Role-based authorization (Guest, HotelOwner, Admin)
- Resource-based authorization (owners manage own resources only)
- HTTPS everywhere, HSTS headers
- Rate limiting on API Gateway (per-IP + per-user)
- Input validation on every command (FluentValidation)
- SQL injection prevention via EF Core parameterized queries
- CORS strict origin whitelist
- Secrets managed via User Secrets (dev) / Azure Key Vault (prod)

---

## 📖 Documentation

- [Architecture Overview](docs/architecture/)
- [Architecture Decision Records](docs/adr/)
- [Database Schemas](docs/database/)
- [API Documentation](docs/diagrams/) — Swagger UI available per service at `/swagger`

---

## 🤝 Contributing

This is a portfolio project by **Semir Tatlı**. For questions or collaboration:

- GitHub: [@semirtatli](https://github.com/semirtatli)
- Email: semirtatli@outlook.com

---

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
