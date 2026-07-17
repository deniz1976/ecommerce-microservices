using System.Net;
using Npgsql;

namespace ECommerce.BuildingBlocks.Persistence;

public static class PostgresConnectionString
{
    public static string CreateLocalDevelopment(string database, string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(database);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = "localhost",
            Port = 5432,
            Database = database,
            Username = username,
            Password = $"{username}_password"
        };

        return builder.ConnectionString;
    }

    public static string Normalize(string connectionString)
    {
        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out Uri? uri) ||
            (uri.Scheme != "postgresql" && uri.Scheme != "postgres"))
        {
            return connectionString;
        }

        string[] userInfo = uri.UserInfo.Split(':', 2);
        string username = userInfo.Length > 0 ? WebUtility.UrlDecode(userInfo[0]) : string.Empty;
        string password = userInfo.Length > 1 ? WebUtility.UrlDecode(userInfo[1]) : string.Empty;
        string database = uri.AbsolutePath.TrimStart('/');
        string query = uri.Query.TrimStart('?');
        Dictionary<string, string> values = ParseQuery(query);

        List<string> parts =
        [
            $"Host={uri.Host}",
            $"Port={(uri.Port > 0 ? uri.Port : 5432)}",
            $"Database={database}",
            $"Username={username}",
            $"Password={password}"
        ];

        if (values.TryGetValue("sslmode", out string? sslMode))
        {
            parts.Add($"SSL Mode={NormalizeSslMode(sslMode)}");
        }

        if (values.TryGetValue("channel_binding", out string? channelBinding))
        {
            parts.Add($"Channel Binding={NormalizeChannelBinding(channelBinding)}");
        }

        return string.Join(';', parts);
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(query))
        {
            return values;
        }

        foreach (string pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] parts = pair.Split('=', 2);
            string key = WebUtility.UrlDecode(parts[0]);
            string value = parts.Length > 1 ? WebUtility.UrlDecode(parts[1]) : string.Empty;
            values[key] = value;
        }

        return values;
    }

    private static string NormalizeSslMode(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "require" => "Require",
            "prefer" => "Prefer",
            "disable" => "Disable",
            "allow" => "Allow",
            "verify-ca" => "VerifyCA",
            "verify-full" => "VerifyFull",
            _ => value
        };
    }

    private static string NormalizeChannelBinding(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "require" => "Require",
            "prefer" => "Prefer",
            "disable" => "Disable",
            _ => value
        };
    }
}
