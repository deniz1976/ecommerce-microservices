using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ECommerce.BuildingBlocks.Persistence;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<CatalogDbContext> builder = new();

        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CatalogDb")
            ?? PostgresConnectionString.CreateLocalDevelopment("catalog_db", "catalog_user");

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new CatalogDbContext(builder.Options);
    }
}
