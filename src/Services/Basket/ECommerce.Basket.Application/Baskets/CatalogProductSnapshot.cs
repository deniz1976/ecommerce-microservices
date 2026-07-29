namespace ECommerce.Basket.Application.Baskets;

public sealed record CatalogProductSnapshot(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    int Status);
