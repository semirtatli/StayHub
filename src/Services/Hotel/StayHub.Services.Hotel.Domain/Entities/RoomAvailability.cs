using StayHub.Shared.Domain;

namespace StayHub.Services.Hotel.Domain.Entities;

/// <summary>
/// Tracks per-date room inventory allocation for a specific room type.
///
/// Each record represents a single calendar date for a room:
///   "On 2026-03-15, Deluxe Room #X has 10 total rooms and 3 are booked."
///
/// This is the inventory-based availability model used by Booking.com-style OTAs:
/// - TotalInventory: physical rooms of this type available on this date
///   (mirrors Room.TotalInventory but can be overridden per date for maintenance, seasonal closures, etc.)
/// - BookedCount: how many are currently reserved
/// - AvailableCount: TotalInventory - BookedCount (computed)
///
/// Concurrency: RowVersion ensures no double-booking when two requests race.
///
/// The entity is NOT an aggregate root — it's owned by the Room (via HotelEntity aggregate).
/// The Hotel Service manages inventory; the Booking Service decrements via commands.
/// </summary>
public sealed class RoomAvailability : Entity
{
    /// <summary>
    /// FK to the Room this availability record belongs to.
    /// </summary>
    public Guid RoomId { get; private init; }

    /// <summary>
    /// The calendar date this record covers (one record per room per date).
    /// </summary>
    public DateOnly Date { get; private init; }

    /// <summary>
    /// Physical rooms available on this date (can differ from Room.TotalInventory
    /// for maintenance blocks, seasonal closures, etc.).
    /// </summary>
    public int TotalInventory { get; private set; }

    /// <summary>
    /// Number of rooms currently booked on this date.
    /// </summary>
    public int BookedCount { get; private set; }

    /// <summary>
    /// Rooms available = TotalInventory - BookedCount.
    /// </summary>
    public int AvailableCount => TotalInventory - BookedCount;

    /// <summary>
    /// Override price for this specific date (seasonal pricing, promotions).
    /// If null, falls back to Room.BasePrice.
    /// </summary>
    public decimal? PriceOverride { get; private set; }

    /// <summary>
    /// Whether this date is blocked (maintenance, owner block, etc.).
    /// Blocked dates cannot be booked regardless of available inventory.
    /// </summary>
    public bool IsBlocked { get; private set; }

    /// <summary>
    /// Reason for blocking (maintenance, renovation, seasonal closure).
    /// </summary>
    public string? BlockReason { get; private set; }

    // ── Factory method ──────────────────────────────────────────────────

    /// <summary>
    /// Creates a new availability record for a specific room on a specific date.
    /// </summary>
    public static RoomAvailability Create(
        Guid roomId,
        DateOnly date,
        int totalInventory)
    {
        if (totalInventory < 0)
            throw new ArgumentOutOfRangeException(
                nameof(totalInventory), "Total inventory cannot be negative.");

        return new RoomAvailability
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            Date = date,
            TotalInventory = totalInventory,
            BookedCount = 0,
            IsBlocked = false
        };
    }

    // ── Behavior methods ────────────────────────────────────────────────

    /// <summary>
    /// Reserve one room on this date. Throws if unavailable.
    /// Called by the Booking Service (via command) when a reservation is confirmed.
    /// </summary>
    public void Reserve()
    {
        if (IsBlocked)
            throw new InvalidOperationException(
                $"Date {Date} is blocked: {BlockReason ?? "No reason specified"}.");

        if (AvailableCount <= 0)
            throw new InvalidOperationException(
                $"No rooms available on {Date}. Booked: {BookedCount}/{TotalInventory}.");

        BookedCount++;
    }

    /// <summary>
    /// Release a reservation on this date (cancellation / no-show).
    /// </summary>
    public void Release()
    {
        if (BookedCount <= 0)
            throw new InvalidOperationException(
                $"Cannot release — no bookings on {Date}.");

        BookedCount--;
    }

    /// <summary>
    /// Update the total inventory for this date (maintenance, seasonal adjustment).
    /// Cannot set below the current BookedCount.
    /// </summary>
    public void UpdateInventory(int newTotalInventory)
    {
        if (newTotalInventory < 0)
            throw new ArgumentOutOfRangeException(
                nameof(newTotalInventory), "Inventory cannot be negative.");

        if (newTotalInventory < BookedCount)
            throw new InvalidOperationException(
                $"Cannot reduce inventory to {newTotalInventory} — " +
                $"{BookedCount} rooms are already booked on {Date}.");

        TotalInventory = newTotalInventory;
    }

    /// <summary>
    /// Set a price override for this specific date.
    /// </summary>
    public void SetPriceOverride(decimal? priceOverride)
    {
        if (priceOverride is < 0)
            throw new ArgumentException("Price override cannot be negative.", nameof(priceOverride));

        PriceOverride = priceOverride.HasValue
            ? Math.Round(priceOverride.Value, 2)
            : null;
    }

    /// <summary>
    /// Block this date (maintenance, closure). Sets available rooms to 0 effective.
    /// </summary>
    public void Block(string? reason = null)
    {
        if (BookedCount > 0)
            throw new InvalidOperationException(
                $"Cannot block {Date} — {BookedCount} rooms are already booked.");

        IsBlocked = true;
        BlockReason = reason;
    }

    /// <summary>
    /// Unblock this date, restoring it to bookable status.
    /// </summary>
    public void Unblock()
    {
        IsBlocked = false;
        BlockReason = null;
    }

    // EF Core parameterless constructor
    private RoomAvailability() { }
}
