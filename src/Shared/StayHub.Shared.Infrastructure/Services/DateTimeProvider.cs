using StayHub.Shared.Interfaces;

namespace StayHub.Shared.Infrastructure.Services;

/// <summary>
/// Production implementation of IDateTimeProvider.
/// Returns DateTime.UtcNow. In tests, substitute with a mock that returns fixed time.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
