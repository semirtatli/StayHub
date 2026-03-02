namespace StayHub.Services.Identity.Domain.Enums;

/// <summary>
/// Application roles for StayHub.
/// Maps to ASP.NET Core Identity roles in the database.
///
/// Role hierarchy:
/// - Guest: Can search hotels, create bookings, write reviews
/// - HotelOwner: All Guest abilities + manage own hotels, rooms, view analytics
/// - Admin: Full system access — manage all users, hotels, platform settings
/// </summary>
public static class AppRoles
{
    public const string Guest = nameof(Guest);
    public const string HotelOwner = nameof(HotelOwner);
    public const string Admin = nameof(Admin);

    /// <summary>
    /// All valid roles for seeding and validation.
    /// </summary>
    public static readonly IReadOnlyList<string> All =
    [
        Guest,
        HotelOwner,
        Admin
    ];
}
