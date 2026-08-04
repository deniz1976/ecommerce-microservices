namespace ECommerce.Inventory.Infrastructure.Catalog;

public sealed class CatalogClientOptions
{
    public const string SectionName = "CatalogClient";

    public string BaseUrl { get; init; } = "http://localhost:5283";

    public int TimeoutSeconds { get; init; } = 5;
}
