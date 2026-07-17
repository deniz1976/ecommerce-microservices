using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<IdentityDbContext> builder = new();

        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__IdentityDb")
            ?? PostgresConnectionString.CreateLocalDevelopment("identity_db", "identity_user");

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new IdentityDbContext(builder.Options);
    }
}
