using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Inventory.Domain;

namespace ECommerce.Inventory.Application.Inventory;

public sealed class InventoryManagementService
{
    private readonly IRepository<InventoryItem, Guid> itemRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IProductInventoryAccessAuthorizer productAccessAuthorizer;

    public InventoryManagementService(
        IRepository<InventoryItem, Guid> itemRepository,
        IUnitOfWork unitOfWork,
        IProductInventoryAccessAuthorizer productAccessAuthorizer)
    {
        this.itemRepository = itemRepository;
        this.unitOfWork = unitOfWork;
        this.productAccessAuthorizer = productAccessAuthorizer;
    }

    public async Task<Result<InventoryItemResponse>> UpsertAsync(
        UpsertInventoryItemRequest request,
        InventoryWriteAccess access,
        CancellationToken cancellationToken)
    {
        if (!access.BypassProductOwnership)
        {
            ProductInventoryAccessResult accessResult = await productAccessAuthorizer.AuthorizeAsync(
                request.ProductId,
                access.AccessToken,
                cancellationToken);
            if (accessResult == ProductInventoryAccessResult.Denied)
            {
                return Failure(ErrorCodes.ProductNotFound);
            }

            if (accessResult == ProductInventoryAccessResult.DependencyUnavailable)
            {
                return Failure(ErrorCodes.DependencyUnavailable);
            }
        }

        InventoryItem? item = await itemRepository.GetByIdAsync(
            request.ProductId,
            cancellationToken);
        if (item is not null && request.QuantityOnHand < item.ReservedQuantity)
        {
            return Failure(ErrorCodes.StockBelowReserved);
        }

        if (item is null)
        {
            item = new InventoryItem(request.ProductId, request.QuantityOnHand);
            itemRepository.Add(item);
        }
        else
        {
            item.SetQuantityOnHand(request.QuantityOnHand);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<InventoryItemResponse>.Success(
            new InventoryItemResponse(
                item.ProductId,
                item.QuantityOnHand,
                item.ReservedQuantity,
                item.AvailableQuantity,
                item.UpdatedAt));
    }

    private static Result<InventoryItemResponse> Failure(string code) =>
        Result<InventoryItemResponse>.Failure(new Error(code, code));
}
