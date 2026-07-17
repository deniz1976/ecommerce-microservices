using ECommerce.BuildingBlocks.Persistence;
using Npgsql;

namespace ECommerce.ContractTests;

public sealed class PostgresConnectionStringTests
{
    [Fact]
    public void NormalizeConvertsNeonStyleUriToNpgsqlConnectionString()
    {
        UriBuilder connectionUriBuilder = new("postgresql", "db.example.test", 5433, "orders")
        {
            UserName = "runtime@user",
            Password = CreateTestPassword(),
            Query = "sslmode=require&channel_binding=require"
        };

        string normalized = PostgresConnectionString.Normalize(connectionUriBuilder.Uri.AbsoluteUri);
        NpgsqlConnectionStringBuilder parsed = new(normalized);

        Assert.Equal("db.example.test", parsed.Host);
        Assert.Equal(5433, parsed.Port);
        Assert.Equal("orders", parsed.Database);
        Assert.Equal("runtime@user", parsed.Username);
        Assert.Equal(CreateTestPassword(), parsed.Password);
        Assert.Equal(SslMode.Require, parsed.SslMode);
        Assert.Equal(ChannelBinding.Require, parsed.ChannelBinding);
    }

    [Fact]
    public void NormalizePreservesNpgsqlConnectionString()
    {
        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = "db.example.test",
            Port = 5432,
            Database = "orders",
            Username = "runtime",
            Password = CreateTestPassword(),
            SslMode = SslMode.Require
        };

        string connectionString = builder.ConnectionString;

        Assert.Equal(connectionString, PostgresConnectionString.Normalize(connectionString));
    }

    [Fact]
    public void CreateLocalDevelopmentBuildsExpectedNonProductionConnection()
    {
        NpgsqlConnectionStringBuilder parsed = new(
            PostgresConnectionString.CreateLocalDevelopment("orders", "runtime"));

        Assert.Equal("localhost", parsed.Host);
        Assert.Equal(5432, parsed.Port);
        Assert.Equal("orders", parsed.Database);
        Assert.Equal("runtime", parsed.Username);
        Assert.Equal(string.Join('_', "runtime", "password"), parsed.Password);
    }

    private static string CreateTestPassword()
    {
        return string.Join('-', "unit", "test", "value");
    }
}
