using MediatR;
using Microsoft.EntityFrameworkCore;
using StayHub.Services.Booking.Domain.Entities;
using StayHub.Shared.Infrastructure.Persistence;

namespace StayHub.Services.Booking.Infrastructure.Persistence;

/// <summary>
/// Booking Service EF Core DbContext.
/// Inherits BaseDbContext which provides:
/// - IUnitOfWork implementation
/// - Domain event dispatching after SaveChanges
/// - Global soft-delete query filters
/// </summary>
public sealed class BookingDbContext : BaseDbContext
{
    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();

    public BookingDbContext(DbContextOptions<BookingDbContext> options, IMediator mediator)
        : base(options, mediator)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
    }
}
