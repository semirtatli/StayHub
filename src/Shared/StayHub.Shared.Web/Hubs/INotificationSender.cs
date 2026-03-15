namespace StayHub.Shared.Web.Hubs;

public interface INotificationSender
{
    Task SendToUserAsync(string userId, string type, object payload, CancellationToken ct = default);
    Task SendToAllAsync(string type, object payload, CancellationToken ct = default);
}
