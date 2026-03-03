using MediatR;
using Microsoft.EntityFrameworkCore;
using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Shared.Infrastructure.Persistence;

namespace StayHub.Services.Hotel.Infrastructure.Persistence;

/// <summary>
/// Hotel Service EF Core DbContext.
/// Inherits BaseDbContext which provides:
/// - IUnitOfWork implementation
/// - Domain event dispatching after SaveChanges
/// - Global soft-delete query filters
/// </summary>
public sealed class HotelDbContext : BaseDbContext
{
    public DbSet<HotelEntity> Hotels => Set<HotelEntity>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomAvailability> RoomAvailability => Set<RoomAvailability>();

    public HotelDbContext(DbContextOptions<HotelDbContext> options, IMediator mediator)
        : base(options, mediator)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
    }
}
