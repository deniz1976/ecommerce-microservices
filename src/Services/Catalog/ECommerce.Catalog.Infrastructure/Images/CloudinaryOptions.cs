namespace ECommerce.Catalog.Infrastructure.Images;

public sealed class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";

    public string? CloudName { get; init; }

    public string? ApiKey { get; init; }

    public string? ApiSecret { get; init; }
}
