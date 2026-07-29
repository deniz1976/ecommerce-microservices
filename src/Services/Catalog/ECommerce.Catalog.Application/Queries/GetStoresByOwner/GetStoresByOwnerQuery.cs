using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.Catalog.Application.Queries.GetStoresByOwner;

public sealed record GetStoresByOwnerQuery(Guid OwnerUserId)
    : IQuery<Result<IReadOnlyCollection<StoreResponse>>>;
