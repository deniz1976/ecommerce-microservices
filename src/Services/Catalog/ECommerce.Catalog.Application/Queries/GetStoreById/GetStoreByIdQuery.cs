using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.Catalog.Application.Queries.GetStoreById;

public sealed record GetStoreByIdQuery(Guid Id) : IQuery<Result<StoreResponse>>;
