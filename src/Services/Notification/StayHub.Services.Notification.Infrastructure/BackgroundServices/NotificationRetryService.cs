using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StayHub.Services.Notification.Application.Features.RetryFailedNotifications;

namespace StayHub.Services.Notification.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically retries failed notifications.
/// Runs every 60 seconds and dispatches RetryFailedNotificationsCommand through MediatR.
/// </summary>
public sealed class NotificationRetryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationRetryService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(60);

    public NotificationRetryService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationRetryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification retry service started. Polling every {Interval}s", _interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                await mediator.Send(new RetryFailedNotificationsCommand(), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification retry cycle");
            }
        }

        _logger.LogInformation("Notification retry service stopped.");
    }
}
