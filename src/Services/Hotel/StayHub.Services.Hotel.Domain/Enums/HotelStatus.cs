namespace StayHub.Services.Hotel.Domain.Enums;

/// <summary>
/// Hotel lifecycle status — follows an approval workflow:
/// Draft → PendingApproval → Active / Rejected → Suspended → Active
///
/// Only Active hotels are visible in search results.
/// </summary>
public enum HotelStatus
{
    /// <summary>Hotel created but not yet submitted for review.</summary>
    Draft = 0,

    /// <summary>Submitted by owner, awaiting admin approval.</summary>
    PendingApproval = 1,

    /// <summary>Approved and visible in search results.</summary>
    Active = 2,

    /// <summary>Temporarily disabled by owner or admin.</summary>
    Suspended = 3,

    /// <summary>Rejected by admin during approval process.</summary>
    Rejected = 4,

    /// <summary>Permanently closed — soft-deleted from listings.</summary>
    Closed = 5
}
