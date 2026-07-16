namespace ECommerce.BuildingBlocks.EventBus;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string ConnectionString { get; init; } = string.Empty;

    public string Host { get; init; } = "localhost";

    public ushort Port { get; init; } = 5672;

    public string Username { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string VirtualHost { get; init; } = "/";

    public bool UseSsl { get; init; }

    public RabbitMqOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            return this;
        }

        Uri uri = new(ConnectionString, UriKind.Absolute);
        string[] userInfo = uri.UserInfo.Split(':', 2);
        string virtualHost = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
        int port = uri.IsDefaultPort
            ? uri.Scheme.Equals("amqps", StringComparison.OrdinalIgnoreCase) ? 5671 : 5672
            : uri.Port;

        return new RabbitMqOptions
        {
            ConnectionString = ConnectionString,
            Host = uri.Host,
            Port = Convert.ToUInt16(port),
            Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : Username,
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : Password,
            VirtualHost = string.IsNullOrWhiteSpace(virtualHost) ? "/" : virtualHost,
            UseSsl = uri.Scheme.Equals("amqps", StringComparison.OrdinalIgnoreCase)
        };
    }
}
