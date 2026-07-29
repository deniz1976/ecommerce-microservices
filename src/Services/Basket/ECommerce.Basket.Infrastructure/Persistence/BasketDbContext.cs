using ECommerce.Basket.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Basket.Infrastructure.Persistence;

public sealed class BasketDbContext : DbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options)
        : base(options)
    {
    }

    public DbSet<BasketCheckoutSnapshot> BasketCheckoutSnapshots => Set<BasketCheckoutSnapshot>();

    public DbSet<BasketCheckoutSnapshotItem> BasketCheckoutSnapshotItems => Set<BasketCheckoutSnapshotItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BasketDbContext).Assembly);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
