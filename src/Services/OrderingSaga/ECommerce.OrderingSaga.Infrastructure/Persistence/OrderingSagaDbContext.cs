using ECommerce.OrderingSaga.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.OrderingSaga.Infrastructure.Persistence;

public sealed class OrderingSagaDbContext : DbContext
{
    public OrderingSagaDbContext(DbContextOptions<OrderingSagaDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderWorkflow> OrderWorkflows => Set<OrderWorkflow>();

    public DbSet<OrderWorkflowItem> OrderWorkflowItems => Set<OrderWorkflowItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingSagaDbContext).Assembly);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
