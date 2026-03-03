using Microsoft.EntityFrameworkCore;
using StayHub.Services.Booking.Domain.Entities;
using StayHub.Services.Booking.Domain.Enums;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Shared.Infrastructure.Persistence;

namespace StayHub.Services.Booking.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IBookingRepository.
/// </summary>
public sealed class BookingRepository : Repository<BookingEntity>, IBookingRepository
{
    public BookingRepository(BookingDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<BookingEntity?> GetByConfirmationNumberAsync(
        string confirmationNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(b => b.ConfirmationNumber == confirmationNumber, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BookingEntity>> GetByGuestUserIdAsync(
        string guestUserId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(b => b.GuestUserId == guestUserId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BookingEntity>> GetByHotelIdAsync(
        Guid hotelId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(b => b.HotelId == hotelId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BookingEntity>> GetByHotelIdAndStatusAsync(
        Guid hotelId,
        BookingStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(b => b.HotelId == hotelId && b.Status == status)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BookingEntity>> GetByRoomAndDateRangeAsync(
        Guid roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(b => b.RoomId == roomId
                && b.StayPeriod.CheckIn < checkOut
                && b.StayPeriod.CheckOut > checkIn)
            .OrderBy(b => b.StayPeriod.CheckIn)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasOverlappingBookingAsync(
        Guid roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken = default)
    {
        // Only check non-terminal bookings (Pending, Confirmed, CheckedIn)
        var activeStatuses = new[]
        {
            BookingStatus.Pending,
            BookingStatus.Confirmed,
            BookingStatus.CheckedIn
        };

        return await DbSet.AnyAsync(
            b => b.RoomId == roomId
                && activeStatuses.Contains(b.Status)
                && b.StayPeriod.CheckIn < checkOut
                && b.StayPeriod.CheckOut > checkIn,
            cancellationToken);
    }
}
