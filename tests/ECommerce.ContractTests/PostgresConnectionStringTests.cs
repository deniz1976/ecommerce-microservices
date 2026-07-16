using ECommerce.BuildingBlocks.Persistence;

namespace ECommerce.ContractTests;

public sealed class PostgresConnectionStringTests
{
    [Fact]
    public void NormalizeConvertsNeonStyleUriToNpgsqlConnectionString()
    {
        string connectionUri = string.Concat(
            "postgresql",
            "://runtime%40user:p%40ss@db.example.test:5433/orders?sslmode=require&channel_binding=require");

        string normalized = PostgresConnectionString.Normalize(connectionUri);

        Assert.Equal(
            "Host=db.example.test;Port=5433;Database=orders;Username=runtime@user;Password=p@ss;SSL Mode=Require;Channel Binding=Require",
            normalized);
    }

    [Fact]
    public void NormalizePreservesNpgsqlConnectionString()
    {
        const string connectionString =
            "Host=db.example.test;Port=5432;Database=orders;Username=runtime;Password=test-value;SSL Mode=Require";

        Assert.Equal(connectionString, PostgresConnectionString.Normalize(connectionString));
    }
}
