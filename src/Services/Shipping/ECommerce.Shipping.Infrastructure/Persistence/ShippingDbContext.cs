using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shipping.Infrastructure.Persistence;

public sealed class ShippingDbContext : DbContext
{
    public ShippingDbContext(DbContextOptions<ShippingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Domain.Shipment> Shipments => Set<Domain.Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShippingDbContext).Assembly);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
