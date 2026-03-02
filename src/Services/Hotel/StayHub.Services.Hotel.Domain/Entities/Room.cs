using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Services.Hotel.Domain.Events;
using StayHub.Services.Hotel.Domain.ValueObjects;
using StayHub.Shared.Domain;

namespace StayHub.Services.Hotel.Domain.Entities;

/// <summary>
/// Room entity — belongs to the Hotel aggregate.
/// Always accessed and modified through the Hotel aggregate root.
///
/// Invariants:
/// - Name cannot be empty.
/// - MaxOccupancy must be at least 1.
/// - BasePrice must be positive.
/// - TotalInventory represents total physical rooms of this type.
///
/// Amenities are stored as a simple list for flexibility.
/// Bed configuration describes the beds available (e.g., "1 King" or "2 Twin").
/// </summary>
public sealed class Room : Entity
{
    private readonly List<string> _amenities = [];

    /// <summary>
    /// Foreign key to the parent Hotel aggregate.
    /// </summary>
    public Guid HotelId { get; private init; }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public RoomType RoomType { get; private set; }

    /// <summary>
    /// Maximum number of guests this room type can accommodate.
    /// </summary>
    public int MaxOccupancy { get; private set; }

    /// <summary>
    /// Base nightly price before any seasonal adjustments or discounts.
    /// Stored as a Money value object (amount + currency).
    /// </summary>
    public Money BasePrice { get; private set; }

    /// <summary>
    /// Total physical rooms of this type in the hotel.
    /// Used for availability calculations.
    /// </summary>
    public int TotalInventory { get; private set; }

    /// <summary>
    /// Room size in square meters.
    /// </summary>
    public decimal? SizeInSquareMeters { get; private set; }

    /// <summary>
    /// Bed configuration description (e.g., "1 King Bed", "2 Twin Beds + 1 Sofa Bed").
    /// </summary>
    public string? BedConfiguration { get; private set; }

    /// <summary>
    /// Whether the room is currently available for booking.
    /// Can be disabled without removing the room.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Room amenities (e.g., "WiFi", "Air Conditioning", "Mini Bar", "Balcony").
    /// </summary>
    public IReadOnlyList<string> Amenities => _amenities.AsReadOnly();

    /// <summary>
    /// Room photo URLs.
    /// </summary>
    private readonly List<string> _photoUrls = [];
    public IReadOnlyList<string> PhotoUrls => _photoUrls.AsReadOnly();

    // ── Factory method ──────────────────────────────────────────────────

    internal static Room Create(
        Guid hotelId,
        string name,
        string description,
        RoomType roomType,
        int maxOccupancy,
        Money basePrice,
        int totalInventory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (maxOccupancy < 1)
            throw new ArgumentOutOfRangeException(nameof(maxOccupancy), "Max occupancy must be at least 1.");

        if (basePrice.IsZero || basePrice.Amount < 0)
            throw new ArgumentException("Base price must be a positive amount.", nameof(basePrice));

        if (totalInventory < 1)
            throw new ArgumentOutOfRangeException(nameof(totalInventory), "Total inventory must be at least 1.");

        return new Room
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId,
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            RoomType = roomType,
            MaxOccupancy = maxOccupancy,
            BasePrice = basePrice,
            TotalInventory = totalInventory,
            IsActive = true
        };
    }

    // ── Behavior methods ────────────────────────────────────────────────

    /// <summary>
    /// Update room details.
    /// </summary>
    public void Update(
        string name,
        string description,
        RoomType roomType,
        int maxOccupancy,
        Money basePrice,
        int totalInventory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (maxOccupancy < 1)
            throw new ArgumentOutOfRangeException(nameof(maxOccupancy), "Max occupancy must be at least 1.");

        if (basePrice.IsZero || basePrice.Amount < 0)
            throw new ArgumentException("Base price must be a positive amount.", nameof(basePrice));

        if (totalInventory < 1)
            throw new ArgumentOutOfRangeException(nameof(totalInventory), "Total inventory must be at least 1.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        RoomType = roomType;
        MaxOccupancy = maxOccupancy;
        BasePrice = basePrice;
        TotalInventory = totalInventory;
    }

    /// <summary>
    /// Set room size in square meters.
    /// </summary>
    public void SetSize(decimal? sizeInSquareMeters)
    {
        if (sizeInSquareMeters is <= 0)
            throw new ArgumentOutOfRangeException(nameof(sizeInSquareMeters), "Room size must be positive.");

        SizeInSquareMeters = sizeInSquareMeters;
    }

    /// <summary>
    /// Set bed configuration description.
    /// </summary>
    public void SetBedConfiguration(string? bedConfiguration)
    {
        BedConfiguration = bedConfiguration?.Trim();
    }

    /// <summary>
    /// Enable or disable the room for booking.
    /// </summary>
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    /// <summary>
    /// Add an amenity to this room (e.g., "WiFi", "Mini Bar").
    /// </summary>
    public void AddAmenity(string amenity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(amenity);
        var trimmed = amenity.Trim();

        if (!_amenities.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
        {
            _amenities.Add(trimmed);
        }
    }

    /// <summary>
    /// Remove an amenity from this room.
    /// </summary>
    public void RemoveAmenity(string amenity)
    {
        var existing = _amenities.FirstOrDefault(a => a.Equals(amenity.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            _amenities.Remove(existing);
        }
    }

    /// <summary>
    /// Replace all amenities at once.
    /// </summary>
    public void SetAmenities(IEnumerable<string> amenities)
    {
        _amenities.Clear();
        foreach (var amenity in amenities)
        {
            AddAmenity(amenity);
        }
    }

    /// <summary>
    /// Add a photo URL for this room.
    /// </summary>
    public void AddPhotoUrl(string photoUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(photoUrl);
        if (!_photoUrls.Contains(photoUrl))
        {
            _photoUrls.Add(photoUrl);
        }
    }

    /// <summary>
    /// Remove a photo URL.
    /// </summary>
    public void RemovePhotoUrl(string photoUrl)
    {
        _photoUrls.Remove(photoUrl);
    }

    // EF Core parameterless constructor
#pragma warning disable CS8618
    private Room() { }
#pragma warning restore CS8618
}
