using Microsoft.EntityFrameworkCore;
using ECommerce.BuildingBlocks.Persistence;

namespace ECommerce.ContractTests;

internal sealed class UtcTimestampTestDbContext : DbContext
{
    public UtcTimestampTestDbContext(DbContextOptions<UtcTimestampTestDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UtcTimestampTestEntity>().HasKey(entity => entity.Id);
        modelBuilder.Entity<UtcTimestampTestEntity>()
            .Property(entity => entity.OccurredAt)
            .IsUtcTimestamp();
    }
}
