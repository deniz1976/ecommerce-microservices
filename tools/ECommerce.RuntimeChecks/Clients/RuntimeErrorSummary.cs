using System.Text.Json;

namespace ECommerce.RuntimeChecks.Clients;

internal static class RuntimeErrorSummary
{
    private const int MaximumResponseBytes = 4096;

    public static async Task<string> ReadAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        await using Stream stream = await content.ReadAsStreamAsync(cancellationToken);
        byte[] buffer = new byte[MaximumResponseBytes + 1];
        int totalRead = 0;

        while (totalRead < buffer.Length)
        {
            int read = await stream.ReadAsync(
                buffer.AsMemory(totalRead, buffer.Length - totalRead),
                cancellationToken);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        if (totalRead == 0 || totalRead > MaximumResponseBytes)
        {
            return "code=unavailable, traceId=unavailable";
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(
                buffer.AsMemory(0, totalRead));
            string code = ReadSafeIdentifier(document.RootElement, "code", 64);
            string traceId = ReadSafeIdentifier(document.RootElement, "traceId", 128);
            return $"code={code}, traceId={traceId}";
        }
        catch (JsonException)
        {
            return "code=unavailable, traceId=unavailable";
        }
    }

    private static string ReadSafeIdentifier(
        JsonElement root,
        string propertyName,
        int maximumLength)
    {
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty(propertyName, out JsonElement property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return "unavailable";
        }

        string? value = property.GetString();
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > maximumLength ||
            value.Any(character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not '-' and not '_' and not '.' and not ':'))
        {
            return "unavailable";
        }

        return value;
    }
}
