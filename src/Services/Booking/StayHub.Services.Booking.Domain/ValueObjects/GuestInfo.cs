namespace StayHub.Services.Booking.Domain.ValueObjects;

/// <summary>
/// Guest information captured at booking time.
/// Stored as a snapshot — the guest's details at the time of booking,
/// independent of any future profile changes.
/// </summary>
public sealed record GuestInfo
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string? Phone { get; }

    /// <summary>
    /// Full name of the guest.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    private GuestInfo(string firstName, string lastName, string email, string? phone)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
    }

    /// <summary>
    /// Create guest info with validation.
    /// </summary>
    public static GuestInfo Create(string firstName, string lastName, string email, string? phone = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new GuestInfo(firstName.Trim(), lastName.Trim(), email.Trim().ToLowerInvariant(), phone?.Trim());
    }

    // EF Core requires a parameterless constructor for owned entities
    private GuestInfo() : this(string.Empty, string.Empty, string.Empty, null) { }
}
