using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.Catalog.Application.Commands.CreateStore;

public sealed record CreateStoreCommand(Guid OwnerUserId, CreateStoreRequest Request)
    : ICommand<Result<StoreResponse>>;
