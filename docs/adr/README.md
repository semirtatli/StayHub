# Architecture Decision Records (ADRs)

ADRs capture the key architectural decisions made for StayHub, along with their context and consequences. Each decision is numbered and immutable — if a decision is reversed, a new ADR is created that supersedes the old one.

## Format

Each ADR follows the [Michael Nygard template](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions):

- **Status**: Proposed → Accepted → Deprecated → Superseded
- **Context**: What situation led to this decision?
- **Decision**: What did we decide?
- **Consequences**: What are the trade-offs?

## Index

| # | Title | Status | Date |
|---|-------|--------|------|
| [001](001-microservices-architecture.md) | Use Microservices Architecture | Accepted | 2026-03-02 |
| [002](002-cqrs-with-mediatr.md) | Use CQRS Pattern with MediatR | Accepted | 2026-03-02 |
| [003](003-database-per-service.md) | Database per Service | Accepted | 2026-03-02 |
| [004](004-async-messaging-rabbitmq-masstransit.md) | Async Messaging with RabbitMQ + MassTransit | Accepted | 2026-03-02 |
| [005](005-yarp-api-gateway.md) | YARP as API Gateway | Accepted | 2026-03-02 |
| [006](006-outbox-pattern.md) | Outbox Pattern for Reliable Messaging | Accepted | 2026-03-02 |
| [007](007-clean-architecture-per-service.md) | Clean Architecture per Service | Accepted | 2026-03-02 |
| [008](008-jwt-authentication.md) | JWT Authentication with Refresh Tokens | Accepted | 2026-03-02 |
