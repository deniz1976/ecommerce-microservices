using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BuildingBlocks.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPostgresDbContext<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionName,
        Action<DbContextOptionsBuilder>? configureOptions = null)
        where TDbContext : DbContext
    {
        string connectionString = configuration.GetConnectionString(connectionName)
            ?? configuration[$"ConnectionStrings:{connectionName}"]
            ?? throw new InvalidOperationException($"Connection string '{connectionName}' was not configured.");

        connectionString = PostgresConnectionString.Normalize(connectionString);

        PostgresOptions options = configuration.GetSection("Postgres").Get<PostgresOptions>() ?? new PostgresOptions();

        services.AddDbContext<TDbContext>(builder =>
        {
            builder.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.CommandTimeout(options.CommandTimeoutSeconds);
                npgsql.EnableRetryOnFailure(options.MaxRetryCount, TimeSpan.FromSeconds(options.MaxRetryDelaySeconds), null);
            });

            configureOptions?.Invoke(builder);
        });

        return services;
    }
}
