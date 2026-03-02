using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Services.Hotel.Domain.Events;
using StayHub.Services.Hotel.Domain.ValueObjects;
using StayHub.Shared.Domain;

namespace StayHub.Services.Hotel.Domain.Entities;

/// <summary>
/// Hotel aggregate root — the core entity of the Hotel bounded context.
///
/// Invariants:
/// - Name cannot be empty.
/// - StarRating must be between 1 and 5.
/// - Address is required.
/// - OwnerId (references Identity Service user) is immutable after creation.
/// - Status follows the approval workflow: Draft → PendingApproval → Active/Rejected → Suspended → Active.
/// - Rooms are managed through the aggregate root to maintain consistency.
///
/// Uses "HotelEntity" instead of "Hotel" to avoid namespace conflict with
/// StayHub.Services.Hotel.Domain namespace.
/// </summary>
public sealed class HotelEntity : AggregateRoot
{
    private readonly List<Room> _rooms = [];

    public string Name { get; private set; }
    public string Description { get; private set; }
    public int StarRating { get; private set; }

    /// <summary>
    /// Physical address — stored as an owned value object in EF Core.
    /// </summary>
    public Address Address { get; private set; }

    /// <summary>
    /// Geographic coordinates for map display and distance search.
    /// </summary>
    public GeoLocation? Location { get; private set; }

    /// <summary>
    /// Contact details (phone, email, website).
    /// </summary>
    public ContactInfo ContactInfo { get; private set; }

    /// <summary>
    /// Identity Service user ID of the hotel owner. Immutable after creation.
    /// </summary>
    public string OwnerId { get; private init; }

    /// <summary>
    /// Hotel lifecycle status — governs visibility and allowed operations.
    /// </summary>
    public HotelStatus Status { get; private set; }

    /// <summary>
    /// Admin remarks when rejecting or suspending the hotel.
    /// </summary>
    public string? StatusReason { get; private set; }

    /// <summary>
    /// Check-in time (e.g., 14:00). Stored as TimeOnly in SQL Server.
    /// </summary>
    public TimeOnly CheckInTime { get; private set; }

    /// <summary>
    /// Check-out time (e.g., 11:00). Stored as TimeOnly in SQL Server.
    /// </summary>
    public TimeOnly CheckOutTime { get; private set; }

    /// <summary>
    /// Primary photo URL for search results / hotel card.
    /// </summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// Rooms belonging to this hotel. Managed through aggregate methods.
    /// </summary>
    public IReadOnlyList<Room> Rooms => _rooms.AsReadOnly();

    // ── Factory method ──────────────────────────────────────────────────

    private HotelEntity(
        string name,
        string description,
        int starRating,
        Address address,
        ContactInfo contactInfo,
        string ownerId,
        TimeOnly checkInTime,
        TimeOnly checkOutTime) : base(Guid.NewGuid())
    {
        Name = name;
        Description = description;
        StarRating = starRating;
        Address = address;
        ContactInfo = contactInfo;
        OwnerId = ownerId;
        Status = HotelStatus.Draft;
        CheckInTime = checkInTime;
        CheckOutTime = checkOutTime;
    }

    public static HotelEntity Create(
        string name,
        string description,
        int starRating,
        Address address,
        ContactInfo contactInfo,
        string ownerId,
        TimeOnly? checkInTime = null,
        TimeOnly? checkOutTime = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(contactInfo);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

        if (starRating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(starRating), "Star rating must be between 1 and 5.");

        var hotel = new HotelEntity(
            name.Trim(),
            description?.Trim() ?? string.Empty,
            starRating,
            address,
            contactInfo,
            ownerId,
            checkInTime ?? new TimeOnly(14, 0),
            checkOutTime ?? new TimeOnly(11, 0));

        hotel.RaiseDomainEvent(new HotelCreatedEvent(hotel.Id, hotel.Name, ownerId));

        return hotel;
    }

    // ── Behavior methods ────────────────────────────────────────────────

    /// <summary>
    /// Update basic hotel information. Only allowed in Draft or Active status.
    /// </summary>
    public void Update(
        string name,
        string description,
        int starRating,
        Address address,
        ContactInfo contactInfo,
        TimeOnly checkInTime,
        TimeOnly checkOutTime)
    {
        if (Status is not (HotelStatus.Draft or HotelStatus.Active))
            throw new InvalidOperationException($"Cannot update hotel in {Status} status.");

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (starRating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(starRating), "Star rating must be between 1 and 5.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        StarRating = starRating;
        Address = address;
        ContactInfo = contactInfo;
        CheckInTime = checkInTime;
        CheckOutTime = checkOutTime;

        RaiseDomainEvent(new HotelUpdatedEvent(Id, Name));
    }

    /// <summary>
    /// Set or update the geographic location.
    /// </summary>
    public void SetLocation(GeoLocation location)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
    }

    /// <summary>
    /// Set the cover image URL.
    /// </summary>
    public void SetCoverImage(string? coverImageUrl)
    {
        CoverImageUrl = coverImageUrl;
    }

    // ── Status workflow ─────────────────────────────────────────────────

    /// <summary>
    /// Submit the hotel for admin approval. Only allowed from Draft or Rejected status.
    /// </summary>
    public void SubmitForApproval()
    {
        if (Status is not (HotelStatus.Draft or HotelStatus.Rejected))
            throw new InvalidOperationException($"Cannot submit for approval from {Status} status.");

        var oldStatus = Status;
        Status = HotelStatus.PendingApproval;
        StatusReason = null;

        RaiseDomainEvent(new HotelStatusChangedEvent(Id, oldStatus, Status, OwnerId, null));
    }

    /// <summary>
    /// Approve the hotel listing. Admin only. Transitions from PendingApproval to Active.
    /// </summary>
    public void Approve(string adminUserId)
    {
        if (Status != HotelStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot approve hotel in {Status} status.");

        var oldStatus = Status;
        Status = HotelStatus.Active;
        StatusReason = null;

        RaiseDomainEvent(new HotelStatusChangedEvent(Id, oldStatus, Status, adminUserId, null));
    }

    /// <summary>
    /// Reject the hotel listing. Admin only. Requires a reason.
    /// </summary>
    public void Reject(string adminUserId, string reason)
    {
        if (Status != HotelStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot reject hotel in {Status} status.");

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var oldStatus = Status;
        Status = HotelStatus.Rejected;
        StatusReason = reason;

        RaiseDomainEvent(new HotelStatusChangedEvent(Id, oldStatus, Status, adminUserId, reason));
    }

    /// <summary>
    /// Suspend the hotel. Can be done by admin or owner.
    /// </summary>
    public void Suspend(string userId, string? reason = null)
    {
        if (Status != HotelStatus.Active)
            throw new InvalidOperationException($"Cannot suspend hotel in {Status} status.");

        var oldStatus = Status;
        Status = HotelStatus.Suspended;
        StatusReason = reason;

        RaiseDomainEvent(new HotelStatusChangedEvent(Id, oldStatus, Status, userId, reason));
    }

    /// <summary>
    /// Reactivate a suspended hotel. Admin only.
    /// </summary>
    public void Reactivate(string adminUserId)
    {
        if (Status != HotelStatus.Suspended)
            throw new InvalidOperationException($"Cannot reactivate hotel in {Status} status.");

        var oldStatus = Status;
        Status = HotelStatus.Active;
        StatusReason = null;

        RaiseDomainEvent(new HotelStatusChangedEvent(Id, oldStatus, Status, adminUserId, null));
    }

    /// <summary>
    /// Permanently close the hotel.
    /// </summary>
    public void Close(string userId, string? reason = null)
    {
        if (Status == HotelStatus.Closed)
            throw new InvalidOperationException("Hotel is already closed.");

        var oldStatus = Status;
        Status = HotelStatus.Closed;
        StatusReason = reason;

        RaiseDomainEvent(new HotelStatusChangedEvent(Id, oldStatus, Status, userId, reason));
    }

    // ── Room management ─────────────────────────────────────────────────

    /// <summary>
    /// Add a room to this hotel. Rooms are managed through the aggregate
    /// to enforce invariants (e.g., unique room names within a hotel).
    /// </summary>
    public Room AddRoom(
        string name,
        string description,
        RoomType roomType,
        int maxOccupancy,
        Money basePrice,
        int totalInventory)
    {
        if (_rooms.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"A room with name '{name}' already exists in this hotel.");

        var room = Room.Create(Id, name, description, roomType, maxOccupancy, basePrice, totalInventory);
        _rooms.Add(room);

        RaiseDomainEvent(new RoomAddedEvent(Id, room.Id, room.Name, room.RoomType));

        return room;
    }

    /// <summary>
    /// Remove a room from the hotel by ID.
    /// </summary>
    public void RemoveRoom(Guid roomId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == roomId)
            ?? throw new InvalidOperationException($"Room with ID '{roomId}' not found in this hotel.");

        _rooms.Remove(room);

        RaiseDomainEvent(new RoomRemovedEvent(Id, roomId));
    }

    /// <summary>
    /// Get a room by ID. Throws if not found.
    /// </summary>
    public Room GetRoom(Guid roomId) =>
        _rooms.FirstOrDefault(r => r.Id == roomId)
            ?? throw new InvalidOperationException($"Room with ID '{roomId}' not found in this hotel.");

    // EF Core parameterless constructor
#pragma warning disable CS8618
    private HotelEntity() { }
#pragma warning restore CS8618
}
