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
            ?? "Host=localhost;Port=5432;Database=ordering_db;Username=ordering_user;Password=ordering_password";

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new OrderingDbContext(builder.Options);
    }
}
