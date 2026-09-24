using ECommerce.Inventory.Application.Inventory;
using ECommerce.Inventory.Domain;

namespace ECommerce.ContractTests;

public sealed class InventoryReservationInvariantTests
{
    [Fact]
    public async Task DuplicateProductLinesAreValidatedAsOneRequestedQuantity()
    {
        Guid productId = Guid.NewGuid();
        InventoryItem item = new(productId, 5);
        item.DequeuePendingMovements();
        FakeInventoryPersistence persistence = new(item);
        StockReservationLoader reservationLoader = new(
            persistence,
            new EmptyStockReservationIdentityReader());
        InventoryReservationService service = new(
            persistence,
            persistence,
            persistence,
            reservationLoader);
        InventoryReservationRequest request = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [
                new InventoryReservationRequestItem(productId, 3),
                new InventoryReservationRequestItem(productId, 3)
            ]);

        InventoryReservationResult result = await service.ReserveAsync(
            request,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, item.ReservedQuantity);
        Assert.Empty(item.PendingMovements);
        Assert.Single(persistence.Reservations);
        Assert.Equal(1, persistence.SaveChangesCount);
    }

    [Fact]
    public async Task RedeliveredReservationAfterReleaseDoesNotReserveStockAgain()
    {
        Guid productId = Guid.NewGuid();
        InventoryItem item = new(productId, 5);
        item.DequeuePendingMovements();
        FakeInventoryPersistence persistence = new(item);
        StockReservationLoader reservationLoader = new(persistence, persistence);
        InventoryReservationService reservationService = new(
            persistence,
            persistence,
            persistence,
            reservationLoader);
        InventoryReleaseService releaseService = new(persistence, persistence, reservationLoader);
        InventoryReservationRequest request = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new InventoryReservationRequestItem(productId, 2)]);

        await reservationService.ReserveAsync(request, CancellationToken.None);
        await releaseService.ReleaseAsync(
            new InventoryReleaseRequest(request.OrderId, request.CustomerId),
            CancellationToken.None);
        InventoryReservationResult redelivered = await reservationService.ReserveAsync(
            request,
            CancellationToken.None);

        Assert.True(redelivered.Succeeded);
        Assert.Equal(0, item.ReservedQuantity);
        Assert.Single(persistence.Reservations);
    }
}
