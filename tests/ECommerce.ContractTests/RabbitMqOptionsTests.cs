using ECommerce.BuildingBlocks.EventBus;

namespace ECommerce.ContractTests;

public sealed class RabbitMqOptionsTests
{
    [Fact]
    public void Normalize_uses_amqps_connection_string_values()
    {
        string connectionString = string.Concat("amqps://", "user-name", ":", "p%40ss-word", "@", "example.rmq.cloudamqp.com", "/", "customer-vhost");

        RabbitMqOptions options = new()
        {
            ConnectionString = connectionString
        };

        RabbitMqOptions normalized = options.Normalize();

        Assert.Equal("example.rmq.cloudamqp.com", normalized.Host);
        Assert.Equal(5671, normalized.Port);
        Assert.Equal("user-name", normalized.Username);
        Assert.Equal("p@ss-word", normalized.Password);
        Assert.Equal("customer-vhost", normalized.VirtualHost);
        Assert.True(normalized.UseSsl);
    }

    [Fact]
    public void Normalize_keeps_explicit_values_when_connection_string_is_empty()
    {
        RabbitMqOptions options = new()
        {
            Host = "localhost",
            Port = 5672,
            Username = "guest",
            Password = "guest",
            VirtualHost = "/",
            UseSsl = false
        };

        RabbitMqOptions normalized = options.Normalize();

        Assert.Same(options, normalized);
    }
}
