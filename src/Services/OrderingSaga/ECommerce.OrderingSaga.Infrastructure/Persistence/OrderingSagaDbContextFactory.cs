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
            ?? "Host=localhost;Port=5432;Database=ordering_saga_db;Username=ordering_saga_user;Password=ordering_saga_password";

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new OrderingSagaDbContext(builder.Options);
    }
}
