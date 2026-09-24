using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Inventory.Domain;

namespace ECommerce.Inventory.Application.Inventory;

public sealed class InventoryReservationService
{
    private readonly IRepository<InventoryItem, Guid> itemRepository;
    private readonly IRepository<StockReservation, Guid> reservationRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly StockReservationLoader reservationLoader;

    public InventoryReservationService(
        IRepository<InventoryItem, Guid> itemRepository,
        IRepository<StockReservation, Guid> reservationRepository,
        IUnitOfWork unitOfWork,
        StockReservationLoader reservationLoader)
    {
        this.itemRepository = itemRepository;
        this.reservationRepository = reservationRepository;
        this.unitOfWork = unitOfWork;
        this.reservationLoader = reservationLoader;
    }

    public async Task<InventoryReservationResult> ReserveAsync(InventoryReservationRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<StockReservation> existingReservations = await reservationLoader.LoadByOrderIdAsync(
            request.OrderId,
            cancellationToken);

        if (existingReservations.Any(x => x.Status is
                StockReservationStatus.Reserved or
                StockReservationStatus.Released or
                StockReservationStatus.Shipped))
        {
            return new InventoryReservationResult(true, null, null);
        }

        if (existingReservations.Any(x => x.Status == StockReservationStatus.Failed))
        {
            return new InventoryReservationResult(false, ErrorCodes.InsufficientStock, existingReservations.First(x => x.Status == StockReservationStatus.Failed).FailureReason);
        }

        Dictionary<Guid, int> requestedQuantities = [];
        foreach (InventoryReservationRequestItem requestItem in request.Items)
        {
            if (requestItem.Quantity <= 0)
            {
                return await AddFailedReservationAsync(
                    request,
                    requestItem,
                    $"Insufficient stock for product {requestItem.ProductId}",
                    cancellationToken);
            }

            int requestedQuantity = requestedQuantities.GetValueOrDefault(requestItem.ProductId);
            try
            {
                requestedQuantity = checked(requestedQuantity + requestItem.Quantity);
            }
            catch (OverflowException)
            {
                return await AddFailedReservationAsync(
                    request,
                    requestItem,
                    "Requested stock quantity is too large.",
                    cancellationToken);
            }

            requestedQuantities[requestItem.ProductId] = requestedQuantity;
            InventoryItem? item = await itemRepository.GetByIdAsync(
                requestItem.ProductId,
                cancellationToken);

            if (item is null || !item.CanReserve(requestedQuantity))
            {
                return await AddFailedReservationAsync(
                    request,
                    requestItem,
                    $"Insufficient stock for product {requestItem.ProductId}",
                    cancellationToken);
            }
        }

        foreach (InventoryReservationRequestItem requestItem in request.Items)
        {
            InventoryItem item = (await itemRepository.GetByIdAsync(
                requestItem.ProductId,
                cancellationToken))!;
            Guid reservationId = Guid.NewGuid();
            InventoryReservationMutationResult mutationResult = item.Reserve(
                requestItem.Quantity,
                request.OrderId,
                reservationId);
            if (mutationResult != InventoryReservationMutationResult.Applied)
            {
                string reason = $"Insufficient stock for product {requestItem.ProductId}";
                return new InventoryReservationResult(
                    false,
                    ErrorCodes.InsufficientStock,
                    reason);
            }

            reservationRepository.Add(new StockReservation(
                reservationId,
                request.OrderId,
                requestItem.ProductId,
                requestItem.Quantity,
                StockReservationStatus.Reserved,
                null));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new InventoryReservationResult(true, null, null);
    }

    private async Task<InventoryReservationResult> AddFailedReservationAsync(
        InventoryReservationRequest request,
        InventoryReservationRequestItem requestItem,
        string reason,
        CancellationToken cancellationToken)
    {
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
