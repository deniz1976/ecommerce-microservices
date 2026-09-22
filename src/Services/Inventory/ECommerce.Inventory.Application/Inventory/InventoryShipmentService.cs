using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Inventory.Domain;

namespace ECommerce.Inventory.Application.Inventory;

public sealed class InventoryShipmentService(
    IRepository<InventoryItem, Guid> itemRepository,
    IUnitOfWork unitOfWork,
    StockReservationLoader reservationLoader)
{
    public async Task ShipAsync(
        InventoryShipmentRequest request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<StockReservation> reservations =
            await reservationLoader.LoadByOrderIdAsync(request.OrderId, cancellationToken);

        foreach (StockReservation reservation in reservations.Where(
            item => item.Status == StockReservationStatus.Reserved))
        {
            InventoryItem? item = await itemRepository.GetByIdAsync(
                reservation.ProductId,
                cancellationToken);
            item?.Ship(reservation.Quantity, request.OrderId, reservation.Id);
            reservation.MarkShipped();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
