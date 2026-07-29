using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Inventory.Application.Commands.ReserveInventory;
using ECommerce.Inventory.Application.Inventory;
using MassTransit;

namespace ECommerce.Inventory.Infrastructure.Messaging;

public sealed class ReserveInventoryConsumer : IConsumer<ReserveInventory>
{
    private readonly ICommandHandler<ReserveInventoryCommand, InventoryReservationResult> commandHandler;

    public ReserveInventoryConsumer(
        ICommandHandler<ReserveInventoryCommand, InventoryReservationResult> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public async Task Consume(ConsumeContext<ReserveInventory> context)
    {
        InventoryReservationRequest request = new(
            context.Message.OrderId,
            context.Message.CustomerId,
            context.Message.Items.Select(x => new InventoryReservationRequestItem(x.ProductId, x.Quantity)).ToArray());

        InventoryReservationResult result = await commandHandler.HandleAsync(
            new ReserveInventoryCommand(request),
            context.CancellationToken);

        if (result.Succeeded)
        {
            await context.Publish(new InventoryReserved(
                Guid.NewGuid(),
                context.Message.CorrelationId,
                context.Message.MessageId,
                DateTimeOffset.UtcNow,
                ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
                context.Message.OrderId,
                context.Message.CustomerId), context.CancellationToken);

            return;
        }

        await context.Publish(new InventoryReservationFailed(
            Guid.NewGuid(),
            context.Message.CorrelationId,
            context.Message.MessageId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            context.Message.OrderId,
            context.Message.CustomerId,
            result.ReasonCode ?? "INSUFFICIENT_STOCK",
            result.Reason ?? "Insufficient stock."), context.CancellationToken);
    }
}
