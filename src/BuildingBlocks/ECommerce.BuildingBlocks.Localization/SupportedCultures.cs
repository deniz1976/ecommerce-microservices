namespace ECommerce.BuildingBlocks.Localization;

public static class SupportedCultures
{
    public const string English = "en";
    public const string Turkish = "tr";
    public const string Default = English;

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        English,
        Turkish
    };

    public static string Normalize(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return Default;
        }

        string twoLetterCulture = culture.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault()?
            .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? Default;

        return All.Contains(twoLetterCulture) ? twoLetterCulture.ToLowerInvariant() : Default;
    }
}
