using Intentum.Persistence.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace Intentum.Persistence.EntityFramework;

/// <summary>
/// Entity Framework DbContext for Intentum persistence.
/// </summary>
public sealed class IntentumDbContext : DbContext
{
    public IntentumDbContext(DbContextOptions<IntentumDbContext> options)
        : base(options)
    {
    }

    public DbSet<BehaviorSpaceEntity> BehaviorSpaces { get; set; } = null!;
    public DbSet<BehaviorEventEntity> BehaviorEvents { get; set; } = null!;
    public DbSet<IntentHistoryEntity> IntentHistory { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BehaviorSpaceEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Events)
                .WithOne(e => e.BehaviorSpace)
                .HasForeignKey(e => e.BehaviorSpaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BehaviorEventEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BehaviorSpaceId);
            entity.HasIndex(e => new { e.Actor, e.Action });
        });

        modelBuilder.Entity<IntentHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BehaviorSpaceId);
            entity.HasIndex(e => e.ConfidenceLevel);
            entity.HasIndex(e => e.Decision);
            entity.HasIndex(e => e.RecordedAt);
        });
    }
}
