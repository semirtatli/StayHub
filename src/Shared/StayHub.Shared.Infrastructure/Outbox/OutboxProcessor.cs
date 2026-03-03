using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StayHub.Shared.Infrastructure.Outbox;

/// <summary>
/// Background service that polls the outbox table for unprocessed messages
/// and publishes them to the message broker.
///
/// Current implementation (Phase 1): Logs events and marks them as processed.
/// Future (Phase 2): Will use MassTransit's IPublishEndpoint to publish to RabbitMQ.
///
/// Generic over TDbContext so each microservice runs its own processor
/// against its own database — maintaining DB-per-service isolation.
///
/// Concurrency note: In a multi-instance deployment, multiple processors may
/// pick up the same message. Consumers must be idempotent to handle this.
/// A future enhancement can use UPDLOCK/READPAST hints or distributed locking.
///
/// Configuration:
/// - Polling interval: 5 seconds
/// - Batch size: 20 messages per cycle
/// - Max retries: 5 (messages exceeding this are logged and skipped)
/// </summary>
public sealed class OutboxProcessor<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor<TDbContext>> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;
    private const int MaxRetryCount = 5;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox processor started for {DbContext}. Polling every {Interval}s",
            typeof(TDbContext).Name, _pollingInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown — expected when the host stops
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor cycle for {DbContext}",
                    typeof(TDbContext).Name);
            }

            try
            {
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation(
            "Outbox processor stopped for {DbContext}", typeof(TDbContext).Name);
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedAtUtc == null && m.RetryCount < MaxRetryCount)
            .OrderBy(m => m.CreatedAtUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return;

        _logger.LogDebug(
            "Processing {Count} outbox message(s) for {DbContext}",
            messages.Count, typeof(TDbContext).Name);

        foreach (var message in messages)
        {
            try
            {
                // TODO: Replace with MassTransit IPublishEndpoint.Publish() when
                // RabbitMQ is configured. For now, log and mark as processed to
                // demonstrate the outbox pattern infrastructure.
                _logger.LogInformation(
                    "Publishing outbox message {MessageId}: {EventType}",
                    message.Id, message.EventType);

                message.MarkAsProcessed();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to publish outbox message {MessageId} (attempt {RetryCount})",
                    message.Id, message.RetryCount + 1);

                message.MarkAsFailed(ex.Message);
            }
        }

        // Persist ProcessedAtUtc / RetryCount updates.
        // This goes through BaseDbContext.SaveChangesAsync but no domain events
        // are collected (OutboxMessage is not an AggregateRoot), so no recursion.
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Extension methods for registering the outbox processor in each service's DI container.
/// </summary>
public static class OutboxRegistration
{
    /// <summary>
    /// Registers the outbox background processor for the specified DbContext.
    /// Call this in each service's InfrastructureRegistration:
    ///   services.AddOutboxProcessor&lt;HotelDbContext&gt;();
    /// </summary>
    public static IServiceCollection AddOutboxProcessor<TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddHostedService<OutboxProcessor<TDbContext>>();
        return services;
    }
}
