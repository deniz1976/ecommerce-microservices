using System.Text.RegularExpressions;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Stores;

public sealed partial class StoreService
{
    private readonly IStoreRepository repository;

    public StoreService(IStoreRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Result<StoreResponse>> CreateAsync(
        Guid ownerUserId,
        CreateStoreRequest request,
        CancellationToken cancellationToken)
    {
        string name = request.Name.Trim();
        string slug = request.Slug.Trim().ToLowerInvariant();
        if (ownerUserId == Guid.Empty || name.Length is < 2 or > 160 ||
            slug.Length is < 2 or > 160 || !StoreSlugRegex().IsMatch(slug))
        {
            return Result<StoreResponse>.Failure(new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
        }

        if (await repository.SlugExistsAsync(slug, cancellationToken))
        {
            return Result<StoreResponse>.Failure(new Error(CatalogErrorCodes.StoreSlugConflict, CatalogErrorCodes.StoreSlugConflict));
        }

        Store store = new(Guid.NewGuid(), ownerUserId, name, slug);
        repository.Add(store);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<StoreResponse>.Success(ToResponse(store));
    }

    public async Task<Result<StoreResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Store? store = await repository.GetByIdAsync(id, cancellationToken);
        return store is null
            ? Result<StoreResponse>.Failure(new Error(CatalogErrorCodes.StoreNotFound, CatalogErrorCodes.StoreNotFound))
            : Result<StoreResponse>.Success(ToResponse(store));
    }

    public async Task<Result<IReadOnlyCollection<StoreResponse>>> GetMineAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Store> stores = await repository.GetByOwnerAsync(ownerUserId, cancellationToken);
        return Result<IReadOnlyCollection<StoreResponse>>.Success(stores.Select(ToResponse).ToArray());
    }

    private static StoreResponse ToResponse(Store store) => new(
        store.Id,
        store.Name,
        store.Slug,
        store.CreatedAt,
        store.UpdatedAt);

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex StoreSlugRegex();
}
