# ADR-006: Outbox Pattern for Reliable Messaging

**Date**: 2026-03-02
**Status**: Accepted

## Context

When a service saves data and publishes an event, two things happen that can fail independently:
1. Database write (e.g., INSERT booking)
2. Message publish (e.g., publish `BookingCreatedEvent` to RabbitMQ)

If the database write succeeds but the message publish fails (network issue, RabbitMQ down), the booking exists but no payment is initiated and no email is sent. This is the **dual-write problem**.

## Options Considered

1. **Hope for the best** — Publish after DB save, accept occasional message loss
2. **Distributed transactions (2PC)** — Coordinate DB and message broker in one transaction
3. **Outbox Pattern** — Write event to a database table in the same transaction as the domain change, then publish asynchronously

## Decision

Use the **Outbox Pattern** via MassTransit's built-in EF Core outbox.

## Rationale

- **Atomicity**: Event and domain change are saved in the same DB transaction — both succeed or both fail
- **At-least-once delivery**: Background job retries until the event is published
- **No distributed transactions**: 2PC is complex, slow, and many brokers don't support it
- **MassTransit built-in**: No custom implementation needed — MassTransit handles the polling, publishing, and cleanup

## How It Works

```
Step 1: Application saves entity + outbox message in ONE transaction
┌─────────────────────────────────────┐
│ BEGIN TRANSACTION                    │
│   INSERT INTO Bookings (...)        │
│   INSERT INTO OutboxMessages (...)  │
│ COMMIT                              │
└─────────────────────────────────────┘

Step 2: MassTransit background job polls OutboxMessages
┌─────────────────────────────────────┐
│ SELECT * FROM OutboxMessages        │
│   WHERE DeliveredAt IS NULL         │
│   ORDER BY CreatedAt                │
│                                     │
│ → Publish to RabbitMQ               │
│ → UPDATE DeliveredAt = UTC_NOW      │
└─────────────────────────────────────┘
```

## Consequences

### Positive
- Zero message loss — if the DB write succeeded, the event will eventually be published
- No distributed transaction coordinator needed
- Works with any message broker (RabbitMQ, Azure Service Bus, etc.)
- MassTransit handles the complexity

### Negative
- **At-least-once** (not exactly-once) — consumers must be idempotent
- Slight delay between DB write and event publish (polling interval)
- OutboxMessages table adds write overhead to every transaction
- Consumers need to handle duplicate messages gracefully
