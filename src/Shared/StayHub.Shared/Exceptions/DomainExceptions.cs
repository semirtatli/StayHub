namespace StayHub.Shared.Exceptions;

/// <summary>
/// Base exception for all domain-level exceptions in StayHub.
/// Thrown when a domain invariant is violated.
/// These should be caught by global exception middleware and mapped to HTTP responses.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// A machine-readable error code for client consumption.
    /// </summary>
    public string ErrorCode { get; }

    protected DomainException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    protected DomainException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Thrown when a requested entity does not exist.
/// Maps to HTTP 404 Not Found.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object EntityId { get; }

    public NotFoundException(string entityName, object entityId)
        : base("ENTITY_NOT_FOUND", $"{entityName} with ID '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}

/// <summary>
/// Thrown when a business rule is violated.
/// Maps to HTTP 422 Unprocessable Entity or 400 Bad Request.
/// </summary>
public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}

/// <summary>
/// Thrown when an operation results in a conflict (e.g., duplicate booking).
/// Maps to HTTP 409 Conflict.
/// </summary>
public sealed class ConflictException : DomainException
{
    public ConflictException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}

/// <summary>
/// Thrown when a user attempts an action they are not authorized for.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public sealed class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base("FORBIDDEN", message)
    {
    }
}

/// <summary>
/// Thrown when optimistic concurrency conflict is detected (stale data).
/// Maps to HTTP 409 Conflict with retry hint.
/// </summary>
public sealed class ConcurrencyException : DomainException
{
    public ConcurrencyException(string entityName, object entityId)
        : base("CONCURRENCY_CONFLICT",
            $"{entityName} with ID '{entityId}' was modified by another user. Please refresh and try again.")
    {
    }
}
