# StayHub — Technology Stack Justification

Every technology choice in StayHub was evaluated against alternatives. This document explains **why** each choice was made — the kind of reasoning expected in architecture reviews and technical interviews.

---

## 1. Backend Runtime: .NET 10

### Chosen: .NET 10 (C# 13)

**Why .NET over alternatives?**

| Criteria | .NET 10 | Node.js (Express/NestJS) | Java (Spring Boot) | Go |
|----------|---------|--------------------------|--------------------|----|
| **Performance** | Top-tier (TechEmpower #1-5) | Good, single-threaded event loop | Good, JVM warmup | Excellent |
| **Type Safety** | Strong (C# static typing) | Weak (TS helps but runtime is JS) | Strong | Strong |
| **Ecosystem** | Rich (NuGet, Azure, EF Core) | Massive (npm) | Rich (Maven) | Growing |
| **Microservice Support** | Excellent (built-in DI, middleware, health checks) | Good | Excellent (Spring Cloud) | Good (manual) |
| **Developer Productivity** | High (LINQ, async/await, source generators) | High | Medium (verbose) | Medium |
| **Azure Integration** | Native (same company) | Good | Good | Good |
| **Hiring Pool** | Large enterprise market | Largest | Largest enterprise | Growing |

**Decision**: .NET 10 provides the best combination of performance, type safety, and Azure integration for this Microsoft-ecosystem project. C# 13 offers modern syntax (primary constructors, collection expressions) that reduces boilerplate.

---

## 2. API Gateway: YARP

### Chosen: YARP (Yet Another Reverse Proxy)

**Alternatives considered:**

| Criteria | YARP | Ocelot | Kong | Nginx |
|----------|------|--------|------|-------|
| **Maintainer** | Microsoft | Community | Kong Inc | Nginx Inc |
| **Performance** | Excellent (Kestrel-based) | Good | Excellent | Excellent |
| **.NET Integration** | Native (ASP.NET Core middleware) | Good | None (.NET) | None (.NET) |
| **Configuration** | C# code or JSON | JSON | YAML/DB | Config files |
| **Customization** | Full ASP.NET Core pipeline | Limited | Plugins (Lua/Go) | Lua/C modules |
| **Active Development** | Very active | Slow | Active | Active |

**Decision**: YARP is Microsoft-built, runs on the ASP.NET Core pipeline (so we can use standard middleware for auth, logging, rate limiting), and is actively maintained. Ocelot's development has slowed. Kong/Nginx require separate technology stacks.

---

## 3. Messaging: RabbitMQ + MassTransit

### Chosen: RabbitMQ as broker, MassTransit as .NET abstraction

**Broker alternatives:**

| Criteria | RabbitMQ | Azure Service Bus | Apache Kafka |
|----------|----------|-------------------|--------------|
| **Cost** | Free (self-hosted) / low (CloudAMQP) | Pay-per-message | Free (self-hosted) / complex |
| **Complexity** | Low-medium | Low (managed) | High |
| **Message Patterns** | Pub/Sub, P2P, routing | Pub/Sub, queues | Stream-based |
| **Best For** | Command/event messaging | Azure-native apps | High-throughput event streaming |
| **Local Dev** | Docker container | Emulator (limited) | Docker (heavy) |

**Why MassTransit over raw RabbitMQ client?**

MassTransit adds:
- **Retry policies** — automatic retry with exponential backoff
- **Dead-letter queues** — failed messages go to error queue for inspection
- **Saga/state machine** — orchestrate multi-step processes (booking flow)
- **Outbox pattern** — built-in support for guaranteed message delivery
- **Serialization** — automatic JSON serialization of messages
- **Consumer testing** — in-memory transport for unit tests

Without MassTransit, we'd manually implement all of this — 2000+ lines of plumbing code.

---

## 4. ORM: Entity Framework Core

### Chosen: EF Core 10 (Code-First)

| Criteria | EF Core | Dapper | Raw ADO.NET |
|----------|---------|--------|-------------|
| **Productivity** | High (LINQ, migrations, change tracking) | Medium | Low |
| **Performance** | Good (compiled queries, split queries) | Excellent | Excellent |
| **Migrations** | Built-in code-first migrations | Manual SQL scripts | Manual |
| **Mapping** | Automatic (conventions + fluent API) | Manual per query | Manual |
| **Interceptors** | Yes (audit, soft-delete automation) | No | No |
| **Learning Curve** | Medium | Low | High |

**Decision**: EF Core provides the best balance of productivity and features. The interceptors are critical — they automatically populate `CreatedAt`, `LastModifiedAt`, `IsDeleted` columns on every save, reducing repetitive code across all 7 services.

**We may use Dapper** selectively for complex read queries in the Analytics Service where raw SQL performance matters.

---

## 5. CQRS + Mediator: MediatR

### Chosen: MediatR 12.x

**Why MediatR?**

| Without MediatR | With MediatR |
|-----------------|--------------|
| Controllers directly call services | Controllers send commands/queries via `IMediator` |
| Cross-cutting logic duplicated in every controller | Pipeline behaviors handle validation, logging, transactions once |
| Hard to test controllers (many dependencies) | Handlers have 1-2 dependencies, easy to test |
| Tight coupling between API and business logic | Complete decoupling — controller doesn't know handler |

**Pipeline behaviors we'll implement:**

```
Request → [Logging] → [Validation] → [Transaction] → Handler → Response
           ↑              ↑               ↑
     Logs every request  Validates via   Wraps command handlers
     with timing         FluentValidation in a DB transaction
```

---

## 6. Authentication: ASP.NET Core Identity + JWT

### Chosen over alternatives:

| Criteria | ASP.NET Core Identity | Azure AD B2C | Duende IdentityServer |
|----------|----------------------|--------------|----------------------|
| **Cost** | Free | Pay per auth | Commercial license |
| **Complexity** | Low | Medium | High |
| **Self-contained** | Yes | No (Azure dependency) | Yes |
| **User Management** | Built-in (registration, password, roles) | Azure Portal | Custom |
| **Customizable** | Fully | Limited | Fully |
| **Demo-friendly** | Very (no external setup) | Needs Azure account | Needs license |

**Decision**: ASP.NET Core Identity gives us full control, zero cost, and works offline. For a portfolio project, it's the most demo-friendly choice.

**JWT strategy:**
- Access token: 15-minute expiry, stored in memory (not localStorage — XSS risk)
- Refresh token: 7-day expiry, stored in httpOnly secure cookie (not accessible via JavaScript)
- Token rotation: Each refresh token use issues a new pair and invalidates the old refresh token

---

## 7. Frontend: React + Vite + TypeScript + Tailwind

### Framework choice:

| Criteria | React | Angular | Vue | Next.js |
|----------|-------|---------|-----|---------|
| **Ecosystem** | Largest | Large | Medium | Large (React-based) |
| **Learning Curve** | Low-medium | High | Low | Medium |
| **Flexibility** | Maximum (pick your tools) | Opinionated (all-in-one) | Medium | Opinionated |
| **Job Market** | Highest demand | Strong (enterprise) | Growing | Growing |
| **SSR Needed?** | No (SPAs are fine for dashboards) | No | No | Built-in |

**Decision**: React 19 — largest ecosystem, highest demand, and user's requirement. We don't need SSR (this isn't a content site — it's a dashboard/booking app), so Next.js overhead isn't justified.

### Build tool: Vite over CRA/Webpack

| Criteria | Vite | Create React App | Webpack manual |
|----------|------|-------------------|----------------|
| **Status** | Active, modern default | Deprecated (2023) | Active but complex |
| **Dev Server** | Instant HMR (native ESM) | Slow (bundled) | Configurable |
| **Build Speed** | Fast (esbuild + Rollup) | Slow | Depends on config |
| **Config** | Minimal | Zero (but inflexible) | Complex |

### CSS: Tailwind over CSS-in-JS/MUI

| Criteria | Tailwind | Material UI | Styled Components |
|----------|----------|-------------|-------------------|
| **Bundle Size** | Tiny (purges unused) | Large (all components) | Medium |
| **Customization** | Full control | "Looks like Material Design" | Full control |
| **Performance** | No runtime overhead | Runtime CSS-in-JS | Runtime overhead |
| **Learning Curve** | Medium (utility classes) | Low (components) | Low (CSS knowledge) |

---

## 8. Database: SQL Server (database-per-service)

### Why SQL Server?

| Criteria | SQL Server | PostgreSQL | MySQL |
|----------|-----------|-----------|-------|
| **Azure Integration** | Native (Azure SQL) | Good (Flexible Server) | Good (Flexible Server) |
| **EF Core Support** | Best (Microsoft develops both) | Very good | Good |
| **Full-text Search** | Built-in | Built-in | Limited |
| **JSON Support** | Good (SQL Server 2022+) | Excellent | Good |
| **Licensing** | Developer/Express free; Azure SQL pay-per-use | Free | Free |
| **Microsoft Ecosystem** | Perfect alignment | Good | Okay |

**Decision**: Project requirement is Microsoft ecosystem end-to-end. SQL Server has the deepest EF Core integration and seamless Azure SQL migration path.

### Why database-per-service (not shared)?

See [ADR-003: Database per Service](../adr/003-database-per-service.md) for the full decision record.

---

## 9. Containerization: Docker + Kubernetes (AKS)

### Why AKS over alternatives?

| Criteria | AKS | Azure App Service | Azure Container Apps |
|----------|-----|-------------------|---------------------|
| **Control** | Full (K8s API) | Limited (PaaS) | Medium |
| **Auto-scaling** | HPA, VPA, cluster autoscaler | Built-in (limited) | KEDA-based |
| **Cost** | Control plane free, pay for nodes | Per-instance | Per-request |
| **Learning Value** | Very high (K8s is industry standard) | Low | Medium |
| **Complexity** | High | Low | Medium |
| **Helm/GitOps** | Yes | No | Limited |

**Decision**: AKS demonstrates Kubernetes expertise (highly valued in the market), provides the most control over scaling and deployment, and the control plane is free. The complexity trade-off is acceptable for a portfolio project that showcases production skills.

---

## 10. Testing Strategy

| Layer | Tool | Why |
|-------|------|-----|
| **Unit Tests** | xUnit | Most popular .NET test framework. `[Theory]` for parameterized tests |
| **Mocking** | NSubstitute | Cleaner syntax than Moq: `sub.Method().Returns(value)` vs lambda setup |
| **Assertions** | FluentAssertions | Readable: `result.Should().Be(42)` vs `Assert.Equal(42, result)` |
| **Integration** | Testcontainers | Real SQL Server + RabbitMQ in Docker during tests — no mocks for infrastructure |
| **E2E** | Playwright | Microsoft-backed, auto-wait, cross-browser, best-in-class for React |

---

*Each technology was chosen to maximize production-readiness, developer experience, and portfolio impressiveness. Alternatives were consciously rejected, not ignored.*
