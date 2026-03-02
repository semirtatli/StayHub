using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Shared.Domain;

namespace StayHub.Shared.Infrastructure.Persistence.Configuration;

/// <summary>
/// Base entity type configuration that applies common mappings for all entities.
/// Every entity configuration in every microservice should inherit from this.
///
/// Example:
///   public class HotelConfiguration : BaseEntityConfiguration&lt;Hotel&gt;
///   {
///       public override void Configure(EntityTypeBuilder&lt;Hotel&gt; builder)
///       {
///           base.Configure(builder); // applies audit columns, key, etc.
///           builder.Property(h => h.Name).HasMaxLength(200).IsRequired();
///           // ...
///       }
///   }
///
/// Configures:
/// - Primary key (Id)
/// - Audit columns (CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
/// - Soft delete columns for AggregateRoot (IsDeleted, DeletedAt, DeletedBy)
/// - Optimistic concurrency token (RowVersion) for AggregateRoot
/// </summary>
public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : Entity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        // Audit columns
        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(256);

        builder.Property(e => e.LastModifiedAt);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(256);

        // If the entity is an AggregateRoot, configure soft delete + concurrency
        if (typeof(AggregateRoot).IsAssignableFrom(typeof(T)))
        {
            ConfigureAggregateRoot(builder);
        }
    }

    private static void ConfigureAggregateRoot(EntityTypeBuilder<T> builder)
    {
        // Soft delete columns
        builder.Property(nameof(ISoftDeletable.IsDeleted))
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(nameof(ISoftDeletable.DeletedAt));

        builder.Property(nameof(ISoftDeletable.DeletedBy))
            .HasMaxLength(256);

        // Optimistic concurrency — SQL Server rowversion
        builder.Property(nameof(AggregateRoot.RowVersion))
            .IsRowVersion();

        // Index on IsDeleted for efficient filtered queries
        builder.HasIndex(nameof(ISoftDeletable.IsDeleted))
            .HasFilter($"[{nameof(ISoftDeletable.IsDeleted)}] = 0")
            .HasDatabaseName($"IX_{typeof(T).Name}_NotDeleted");
    }
}
