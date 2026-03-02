# ADR-002: Use CQRS Pattern with MediatR

**Date**: 2026-03-02
**Status**: Accepted

## Context

Each microservice needs an internal application architecture pattern. Controllers need to delegate work to business logic without becoming bloated. We also need cross-cutting concerns (validation, logging, transactions) applied consistently.

## Options Considered

1. **Traditional Service Layer** — Controllers call injected service classes directly
2. **CQRS with MediatR** — Commands and queries as message objects, handlers via mediator
3. **Vertical Slice Architecture** — Each feature is a self-contained slice (handler + model + validation)

## Decision

Use **CQRS with MediatR** and pipeline behaviors.

## Rationale

- **Separation of reads and writes**: Read queries bypass domain model for performance (project directly to DTOs); write commands go through full domain validation
- **Pipeline behaviors**: Validation, logging, and transaction management implemented once and applied to every request automatically
- **Testability**: Each handler has 1-2 dependencies, trivially unit-testable
- **Decoupling**: Controllers know nothing about domain or infrastructure — they only know `IMediator`
- **Consistency**: Every use case follows the same pattern: Command/Query → Validator → Handler → Result

## Consequences

### Positive
- Controllers are thin (5-10 lines per action)
- Cross-cutting concerns are centralized in behaviors
- Each use case is a single class — easy to find, test, and modify
- Read side can be optimized independently (Dapper, raw SQL, projections)

### Negative
- More files per feature (Command + Validator + Handler + Response DTO)
- Indirection — `IMediator.Send()` makes call stack less obvious
- MediatR adds a dependency to every Application layer

## Implementation

```
Request Flow:
Controller → IMediator.Send(command)
  → LoggingBehavior (log entry/exit + timing)
  → ValidationBehavior (FluentValidation, throws on failure)
  → TransactionBehavior (wraps in DbTransaction for commands)
  → Handler (business logic)
  → Result<T> response
```
