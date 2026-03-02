# ADR-001: Use Microservices Architecture

**Date**: 2026-03-02
**Status**: Accepted

## Context

StayHub is a hotel marketplace (OTA) with distinct business domains: identity, hotel management, booking, payments, reviews, notifications, and analytics. We need to decide on the overall system architecture style.

## Options Considered

1. **Monolith** — Single deployable unit, shared database
2. **Modular Monolith** — Single deployable unit, well-defined module boundaries
3. **Microservices** — Independent services per bounded context, separate databases

## Decision

Use **microservices architecture** with one service per bounded context.

## Rationale

- **Independent scaling**: Booking Service needs 10x more capacity during peak seasons than Review Service
- **Failure isolation**: Payment Service outage shouldn't prevent hotel browsing
- **Independent deployment**: Hotel search improvements deploy without touching payment logic
- **Technology showcase**: Demonstrates distributed systems expertise (messaging, service discovery, resilience patterns)
- **Team simulation**: Each service could be owned by a different team in a real organization

## Consequences

### Positive
- Services scale independently based on load
- Fault isolation — one service failure doesn't cascade
- Independent deployments reduce release risk
- Clear ownership boundaries per domain

### Negative
- Distributed transactions are complex — requires Outbox Pattern (see ADR-006)
- Network latency between services vs. in-process calls
- Operational overhead: 7 services × (deployment + monitoring + logging)
- Data consistency is eventual, not immediate
- More complex local development setup (Docker Compose required)

## Compliance

All inter-service communication must use either:
- **Synchronous HTTP** — for queries requiring immediate response
- **Asynchronous events via RabbitMQ** — for commands and domain events (see ADR-004)

No service may directly access another service's database.
