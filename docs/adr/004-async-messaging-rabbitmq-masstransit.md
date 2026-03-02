# ADR-004: Async Messaging with RabbitMQ + MassTransit

**Date**: 2026-03-02
**Status**: Accepted

## Context

Microservices need to communicate for cross-domain workflows (e.g., booking created → initiate payment → send email). We need to decide between synchronous and asynchronous patterns and choose a message broker.

## Options Considered

### Broker
1. **RabbitMQ** — Traditional message broker, AMQP protocol
2. **Apache Kafka** — Distributed event streaming platform
3. **Azure Service Bus** — Managed Azure messaging service

### .NET Abstraction
1. **Raw RabbitMQ.Client** — Direct AMQP client
2. **MassTransit** — High-level abstraction over message brokers
3. **NServiceBus** — Commercial service bus framework

## Decision

Use **RabbitMQ** as the broker and **MassTransit** as the .NET abstraction layer.

## Rationale

### RabbitMQ over alternatives
- **Free and self-hosted**: No cloud vendor lock-in, runs in Docker locally
- **Lightweight**: Perfect for command/event messaging (we don't need Kafka's stream semantics)
- **Mature**: Battle-tested in production by thousands of companies
- **Local dev friendly**: Docker container starts in seconds

### MassTransit over raw client
- **Retry + dead-letter**: Automatic retry with exponential backoff; failed messages go to error queue
- **Outbox pattern**: Built-in EF Core outbox for reliable messaging (see ADR-006)
- **Saga support**: State machines for complex workflows like booking flow
- **Testability**: In-memory transport for unit tests — no RabbitMQ needed
- **Serialization**: Automatic message serialization/deserialization
- **Broker abstraction**: Can switch from RabbitMQ to Azure Service Bus by changing one line

## Consequences

### Positive
- Services are decoupled — publisher doesn't know consumers
- Naturally handles spikes (messages queue up, consumers process at their pace)
- Failed message processing doesn't lose data (dead-letter queue)
- Easy to add new consumers for existing events

### Negative
- Eventual consistency — consumer might process event seconds/minutes after publish
- Message ordering not guaranteed across consumers
- Debugging async flows is harder than synchronous calls
- Additional infrastructure to maintain (RabbitMQ server)

## Message Conventions

```
Integration Events: StayHub.Shared.IntegrationEvents namespace
Naming: Past tense — BookingCreatedEvent, PaymentCompletedEvent
Queues: Auto-created by MassTransit based on consumer type names
```
