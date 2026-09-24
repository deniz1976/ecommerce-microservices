using StackExchange.Redis;
using Testcontainers.Redis;

namespace ECommerce.Basket.IntegrationTests;

public sealed class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer container = new RedisBuilder("redis:7-alpine").Build();

    public IConnectionMultiplexer Connection { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await container.StartAsync();
        Connection = await ConnectionMultiplexer.ConnectAsync(container.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await Connection.DisposeAsync();
        await container.DisposeAsync();
    }
}
