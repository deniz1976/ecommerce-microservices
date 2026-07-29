namespace ECommerce.Catalog.Application.Metrics;

public sealed record CatalogMetricsResponse(
    long TotalProducts,
    long ActiveProducts,
    long DraftProducts,
    long InactiveProducts,
    long ArchivedProducts,
    long TotalStores,
    long TotalCategories,
    long TotalBrands);
