using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.Inventory.Domain;

namespace ECommerce.Inventory.Application.Inventory;

public sealed class InventoryService
{
    private readonly IInventoryRepository repository;

    public InventoryService(IInventoryRepository repository)
    {
        this.repository = repository;
    }

    public async Task<InventoryReservationResult> ReserveAsync(InventoryReservationRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<StockReservation> existingReservations = await repository.GetReservationsAsync(request.OrderId, cancellationToken);

        if (existingReservations.Any(x => x.Status == StockReservationStatus.Reserved))
        {
            return new InventoryReservationResult(true, null, null);
        }

        if (existingReservations.Any(x => x.Status == StockReservationStatus.Failed))
        {
            return new InventoryReservationResult(false, ErrorCodes.InsufficientStock, existingReservations.First(x => x.Status == StockReservationStatus.Failed).FailureReason);
        }

        foreach (InventoryReservationRequestItem requestItem in request.Items)
        {
            InventoryItem? item = await repository.GetItemAsync(requestItem.ProductId, cancellationToken);

            if (item is null || !item.CanReserve(requestItem.Quantity))
            {
                string reason = $"Insufficient stock for product {requestItem.ProductId}";
                repository.AddReservation(new StockReservation(Guid.NewGuid(), request.OrderId, requestItem.ProductId, requestItem.Quantity, StockReservationStatus.Failed, reason));
                await repository.SaveChangesAsync(cancellationToken);
                return new InventoryReservationResult(false, ErrorCodes.InsufficientStock, reason);
            }
        }

        foreach (InventoryReservationRequestItem requestItem in request.Items)
        {
            InventoryItem item = (await repository.GetItemAsync(requestItem.ProductId, cancellationToken))!;
            item.Reserve(requestItem.Quantity);
            repository.AddReservation(new StockReservation(Guid.NewGuid(), request.OrderId, requestItem.ProductId, requestItem.Quantity, StockReservationStatus.Reserved, null));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return new InventoryReservationResult(true, null, null);
    }

    public async Task<InventoryItemResponse> UpsertAsync(UpsertInventoryItemRequest request, CancellationToken cancellationToken)
    {
        InventoryItem? item = await repository.GetItemAsync(request.ProductId, cancellationToken);

        if (item is null)
        {
            item = new InventoryItem(request.ProductId, request.QuantityOnHand);
            repository.AddItem(item);
        }
        else
        {
            item.SetQuantityOnHand(request.QuantityOnHand);
        }

        await repository.SaveChangesAsync(cancellationToken);
        return new InventoryItemResponse(item.ProductId, item.QuantityOnHand, item.ReservedQuantity, item.AvailableQuantity, item.UpdatedAt);
    }

    public async Task<InventoryItemResponse?> GetItemAsync(Guid productId, CancellationToken cancellationToken)
    {
        InventoryItem? item = await repository.GetItemAsync(productId, cancellationToken);

        return item is null
            ? null
            : new InventoryItemResponse(item.ProductId, item.QuantityOnHand, item.ReservedQuantity, item.AvailableQuantity, item.UpdatedAt);
    }

    public async Task ReleaseAsync(InventoryReleaseRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<StockReservation> reservations = await repository.GetReservationsAsync(request.OrderId, cancellationToken);

        foreach (StockReservation reservation in reservations.Where(x => x.Status == StockReservationStatus.Reserved))
        {
            InventoryItem? item = await repository.GetItemAsync(reservation.ProductId, cancellationToken);
            item?.Release(reservation.Quantity);
            reservation.MarkReleased();
        }

        await repository.SaveChangesAsync(cancellationToken);
    }
}
