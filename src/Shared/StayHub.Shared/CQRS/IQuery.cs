using MediatR;
using StayHub.Shared.Result;

namespace StayHub.Shared.CQRS;

/// <summary>
/// Marker interface for queries (read operations).
/// Queries do NOT modify state — they project data into DTOs.
/// They bypass the domain model and read directly from the database for performance.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
