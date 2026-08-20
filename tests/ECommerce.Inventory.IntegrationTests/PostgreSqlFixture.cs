using Testcontainers.PostgreSql;

namespace ECommerce.Inventory.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("inventory_tests")
        .WithUsername("inventory_tests")
        .WithPassword("inventory_tests")
        .Build();

    public string ConnectionString => container.GetConnectionString();

    public Task InitializeAsync() => container.StartAsync();

    public Task DisposeAsync() => container.DisposeAsync().AsTask();
}
