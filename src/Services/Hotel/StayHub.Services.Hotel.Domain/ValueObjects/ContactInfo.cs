using System.Text.RegularExpressions;
using StayHub.Shared.Domain;

namespace StayHub.Services.Hotel.Domain.ValueObjects;

/// <summary>
/// Contact information for a hotel — phone, email, website.
/// Value object with basic format validation.
/// </summary>
public sealed partial class ContactInfo : ValueObject
{
    public string Phone { get; private init; }
    public string Email { get; private init; }
    public string? Website { get; private init; }

    private ContactInfo(string phone, string email, string? website)
    {
        Phone = phone;
        Email = email;
        Website = website;
    }

    public static ContactInfo Create(string phone, string email, string? website = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (!EmailRegex().IsMatch(email))
            throw new ArgumentException("Invalid email address format.", nameof(email));

        return new ContactInfo(phone.Trim(), email.Trim().ToLowerInvariant(), website?.Trim());
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Phone;
        yield return Email;
        yield return Website;
    }

    // EF Core parameterless constructor
#pragma warning disable CS8618
    private ContactInfo() { }
#pragma warning restore CS8618
}
