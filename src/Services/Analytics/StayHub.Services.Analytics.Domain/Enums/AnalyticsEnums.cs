namespace StayHub.Services.Analytics.Domain.Enums;

/// <summary>
/// Types of analytics events tracked by the projection engine.
/// Each type maps to a specific integration event from another service.
/// </summary>
public enum AnalyticsEventType
{
    BookingConfirmed = 0,
    BookingCancelled = 1,
    PaymentReceived = 2,
    RefundProcessed = 3,
    ReviewSubmitted = 4
}

/// <summary>
/// Time granularity for trend aggregation queries.
/// Controls how data points are grouped in time-series responses.
/// </summary>
public enum TimePeriod
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Yearly = 3
}
