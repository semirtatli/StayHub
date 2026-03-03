using MediatR;
using Microsoft.EntityFrameworkCore;
using StayHub.Services.Payment.Domain.Entities;
using StayHub.Shared.Infrastructure.Persistence;

namespace StayHub.Services.Payment.Infrastructure.Persistence;

/// <summary>
/// Payment Service EF Core DbContext.
/// Inherits BaseDbContext which provides:
/// - IUnitOfWork implementation
/// - Domain event dispatching after SaveChanges
/// - Outbox message persistence
/// - Global soft-delete query filters
/// </summary>
public sealed class PaymentDbContext : BaseDbContext
{
    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options, IMediator mediator)
        : base(options, mediator)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
    }
}
