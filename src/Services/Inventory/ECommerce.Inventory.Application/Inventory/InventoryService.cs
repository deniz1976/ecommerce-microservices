using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Inventory.Domain;

namespace ECommerce.Inventory.Application.Inventory;

public sealed class InventoryService
{
    private readonly IRepository<InventoryItem, Guid> itemRepository;
    private readonly IRepository<StockReservation, Guid> reservationRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IStockReservationIdentityReader reservationIdentityReader;

    public InventoryService(
        IRepository<InventoryItem, Guid> itemRepository,
        IRepository<StockReservation, Guid> reservationRepository,
        IUnitOfWork unitOfWork,
        IStockReservationIdentityReader reservationIdentityReader)
    {
        this.itemRepository = itemRepository;
        this.reservationRepository = reservationRepository;
        this.unitOfWork = unitOfWork;
        this.reservationIdentityReader = reservationIdentityReader;
    }

    public async Task<InventoryReservationResult> ReserveAsync(InventoryReservationRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<StockReservation> existingReservations = await GetReservationsAsync(
            request.OrderId,
            cancellationToken);

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
            InventoryItem? item = await itemRepository.GetByIdAsync(
                requestItem.ProductId,
                cancellationToken);

            if (item is null || !item.CanReserve(requestItem.Quantity))
            {
                string reason = $"Insufficient stock for product {requestItem.ProductId}";
                reservationRepository.Add(new StockReservation(
                    Guid.NewGuid(),
                    request.OrderId,
                    requestItem.ProductId,
                    requestItem.Quantity,
                    StockReservationStatus.Failed,
                    reason));
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return new InventoryReservationResult(false, ErrorCodes.InsufficientStock, reason);
            }
        }

        foreach (InventoryReservationRequestItem requestItem in request.Items)
        {
            InventoryItem item = (await itemRepository.GetByIdAsync(
                requestItem.ProductId,
                cancellationToken))!;
            item.Reserve(requestItem.Quantity);
            reservationRepository.Add(new StockReservation(
                Guid.NewGuid(),
                request.OrderId,
                requestItem.ProductId,
                requestItem.Quantity,
                StockReservationStatus.Reserved,
                null));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new InventoryReservationResult(true, null, null);
    }

    public async Task<InventoryItemResponse> UpsertAsync(UpsertInventoryItemRequest request, CancellationToken cancellationToken)
    {
        InventoryItem? item = await itemRepository.GetByIdAsync(
            request.ProductId,
            cancellationToken);

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
        return new InventoryItemResponse(item.ProductId, item.QuantityOnHand, item.ReservedQuantity, item.AvailableQuantity, item.UpdatedAt);
    }

    public async Task<InventoryItemResponse?> GetItemAsync(Guid productId, CancellationToken cancellationToken)
    {
        InventoryItem? item = await itemRepository.GetByIdAsync(productId, cancellationToken);

        return item is null
            ? null
            : new InventoryItemResponse(item.ProductId, item.QuantityOnHand, item.ReservedQuantity, item.AvailableQuantity, item.UpdatedAt);
    }

    public async Task ReleaseAsync(InventoryReleaseRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<StockReservation> reservations = await GetReservationsAsync(
            request.OrderId,
            cancellationToken);

        foreach (StockReservation reservation in reservations.Where(x => x.Status == StockReservationStatus.Reserved))
        {
            InventoryItem? item = await itemRepository.GetByIdAsync(
                reservation.ProductId,
                cancellationToken);
            item?.Release(reservation.Quantity);
            reservation.MarkReleased();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<StockReservation>> GetReservationsAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid> reservationIds =
            await reservationIdentityReader.FindIdsByOrderIdAsync(orderId, cancellationToken);
        List<StockReservation> reservations = new(reservationIds.Count);
        foreach (Guid reservationId in reservationIds)
        {
            StockReservation? reservation = await reservationRepository.GetByIdAsync(
                reservationId,
                cancellationToken);
            if (reservation is not null)
            {
                reservations.Add(reservation);
            }
        }

        return reservations;
    }
}
