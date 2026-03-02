using MediatR;
using StayHub.Shared.Result;

namespace StayHub.Shared.CQRS;

/// <summary>
/// Marker interface for command handlers.
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result.Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Command handler that returns a typed result.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
