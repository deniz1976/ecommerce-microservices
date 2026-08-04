using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.Catalog.Application.Commands.UpdateStore;

public sealed record UpdateStoreCommand(
    Guid StoreId,
    UpdateStoreRequest Request,
    StoreAccessContext Access) : ICommand<Result<StoreResponse>>;
