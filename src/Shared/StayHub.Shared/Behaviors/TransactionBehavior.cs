using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Shared.CQRS;
using StayHub.Shared.Interfaces;
using StayHub.Shared.Result;

namespace StayHub.Shared.Behaviors;

/// <summary>
/// MediatR pipeline behavior that wraps command handlers in a unit-of-work transaction.
///
/// How it works:
/// 1. Only runs for commands (ICommandBase marker) — queries are read-only, no transaction needed
/// 2. Calls the handler (which may modify entities, raise domain events)
/// 3. If the handler returns success → commits via IUnitOfWork.SaveChangesAsync()
/// 4. If the handler returns failure → does NOT commit (changes are discarded)
///
/// This keeps handlers clean: they mutate aggregates and return Result,
/// and this behavior handles the persistence/commit concern.
///
/// Note: EF Core's change tracker acts as a natural transaction scope.
/// SaveChangesAsync wraps all tracked changes in a single SQL transaction.
/// For cross-aggregate operations, use explicit transactions in the handler.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandBase, IRequest<TResponse>
    where TResponse : Result.Result
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogDebug(
            "Beginning transaction for {RequestName}",
            requestName);

        var response = await next();

        if (response.IsFailure)
        {
            _logger.LogDebug(
                "Skipping commit for {RequestName} — handler returned failure: {ErrorCode}",
                requestName,
                response.Error.Code);

            return response;
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogDebug(
                "Committed transaction for {RequestName}",
                requestName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Transaction commit failed for {RequestName}",
                requestName);

            throw;
        }

        return response;
    }
}
