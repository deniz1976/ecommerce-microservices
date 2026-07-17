using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<OrderingDbContext> builder = new();

        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__OrderingDb")
            ?? PostgresConnectionString.CreateLocalDevelopment("ordering_db", "ordering_user");

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new OrderingDbContext(builder.Options);
    }
}
