using Microsoft.AspNetCore.SignalR;

namespace StayHub.Shared.Web.Hubs;

public sealed class SignalRNotificationSender : INotificationSender
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationSender(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(string userId, string type, object payload, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group($"user-{userId}")
            .SendAsync("ReceiveNotification", new { type, payload, timestamp = DateTime.UtcNow }, ct);
    }

    public async Task SendToAllAsync(string type, object payload, CancellationToken ct = default)
    {
        await _hubContext.Clients.All
            .SendAsync("ReceiveNotification", new { type, payload, timestamp = DateTime.UtcNow }, ct);
    }
}
