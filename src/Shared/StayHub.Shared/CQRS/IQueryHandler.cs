using MediatR;
using StayHub.Shared.Result;

namespace StayHub.Shared.CQRS;

/// <summary>
/// Query handler that returns a typed result.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
