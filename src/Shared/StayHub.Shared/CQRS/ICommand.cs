using MediatR;
using StayHub.Shared.Result;

namespace StayHub.Shared.CQRS;

/// <summary>
/// Marker interface for commands (write operations).
/// Commands modify state and return Result (no value) or Result&lt;T&gt; (with value).
/// Examples: CreateBookingCommand, CancelBookingCommand.
/// </summary>
public interface ICommand : IRequest<Result.Result>
{
}

/// <summary>
/// Command that returns a typed result on success.
/// Example: CreateBookingCommand : ICommand&lt;Guid&gt; returns the new booking ID.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
