using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.OrderingSaga.Infrastructure.Persistence;

public sealed class OrderingSagaDbContextFactory : IDesignTimeDbContextFactory<OrderingSagaDbContext>
{
    public OrderingSagaDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<OrderingSagaDbContext> builder = new();

        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__OrderingSagaDb")
            ?? PostgresConnectionString.CreateLocalDevelopment("ordering_saga_db", "ordering_saga_user");

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new OrderingSagaDbContext(builder.Options);
    }
}
