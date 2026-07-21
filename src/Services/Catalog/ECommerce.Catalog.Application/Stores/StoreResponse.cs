namespace ECommerce.Catalog.Application.Stores;

public sealed record StoreResponse(
    Guid Id,
    string Name,
    string Slug,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
