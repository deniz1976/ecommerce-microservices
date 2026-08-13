using System.Text.RegularExpressions;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Stores;

public sealed partial class StoreManagementService(
    IRepository<Store, Guid> repository,
    IUnitOfWork unitOfWork,
    IStoreReader storeReader)
{
    public async Task<Result<StoreResponse>> CreateAsync(
        Guid ownerUserId,
        CreateStoreRequest request,
        CancellationToken cancellationToken)
    {
        string name = request.Name.Trim();
        string slug = request.Slug.Trim().ToLowerInvariant();
        if (ownerUserId == Guid.Empty || !IsValid(name, slug))
        {
            return ValidationFailure();
        }

        if (await storeReader.SlugExistsAsync(slug, cancellationToken))
        {
            return SlugConflict();
        }

        Store store = new(Guid.NewGuid(), ownerUserId, name, slug);
        repository.Add(store);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<StoreResponse>.Success(StoreResponseMapper.ToResponse(store));
    }

    public async Task<Result<StoreResponse>> UpdateAsync(
        Guid storeId,
        UpdateStoreRequest request,
        StoreAccessContext access,
        CancellationToken cancellationToken)
    {
        Store? store = await repository.GetByIdAsync(storeId, cancellationToken);
        if (store is null ||
            (!access.IsAdmin && (access.UserId is null || store.OwnerUserId != access.UserId)))
        {
            return Result<StoreResponse>.Failure(new Error(
                CatalogErrorCodes.StoreNotFound,
                CatalogErrorCodes.StoreNotFound));
        }

        string name = request.Name.Trim();
        string slug = request.Slug.Trim().ToLowerInvariant();
        if (!IsValid(name, slug))
        {
            return ValidationFailure();
        }

        if (!string.Equals(store.Slug, slug, StringComparison.Ordinal) &&
            await storeReader.SlugExistsAsync(slug, cancellationToken))
        {
            return SlugConflict();
        }

        store.Update(name, slug, DateTimeOffset.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<StoreResponse>.Success(StoreResponseMapper.ToResponse(store));
    }

    private static bool IsValid(string name, string slug) =>
        name.Length is >= 2 and <= 160 &&
        slug.Length is >= 2 and <= 160 &&
        StoreSlugRegex().IsMatch(slug);

    private static Result<StoreResponse> ValidationFailure() =>
        Result<StoreResponse>.Failure(new Error(
            ErrorCodes.ValidationFailed,
            ErrorCodes.ValidationFailed));

    private static Result<StoreResponse> SlugConflict() =>
        Result<StoreResponse>.Failure(new Error(
            CatalogErrorCodes.StoreSlugConflict,
            CatalogErrorCodes.StoreSlugConflict));

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex StoreSlugRegex();
}
