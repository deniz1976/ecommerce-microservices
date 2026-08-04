using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.Catalog.Application.Commands.UpdateStore;

public sealed class UpdateStoreCommandHandler
    : ICommandHandler<UpdateStoreCommand, Result<StoreResponse>>
{
    private readonly StoreService storeService;

    public UpdateStoreCommandHandler(StoreService storeService)
    {
        this.storeService = storeService;
    }

    public Task<Result<StoreResponse>> HandleAsync(
        UpdateStoreCommand command,
        CancellationToken cancellationToken)
    {
        return storeService.UpdateAsync(
            command.StoreId,
            command.Request,
            command.Access,
            cancellationToken);
    }
}
