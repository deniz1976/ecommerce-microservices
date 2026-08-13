using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.Catalog.Application.Commands.CreateStore;

public sealed class CreateStoreCommandHandler
    : ICommandHandler<CreateStoreCommand, Result<StoreResponse>>
{
    private readonly StoreManagementService storeService;

    public CreateStoreCommandHandler(StoreManagementService storeService)
    {
        this.storeService = storeService;
    }

    public Task<Result<StoreResponse>> HandleAsync(
        CreateStoreCommand command,
        CancellationToken cancellationToken)
    {
        return storeService.CreateAsync(
            command.OwnerUserId,
            command.Request,
            cancellationToken);
    }
}
