namespace ECommerce.Catalog.Application.Stores;

public sealed record ManagedStoreResponse(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    string Slug,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
