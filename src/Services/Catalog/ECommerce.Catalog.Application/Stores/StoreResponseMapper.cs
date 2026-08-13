using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Stores;

internal static class StoreResponseMapper
{
    public static StoreResponse ToResponse(Store store) => new(
        store.Id,
        store.Name,
        store.Slug,
        store.CreatedAt,
        store.UpdatedAt);
}
