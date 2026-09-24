using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.Shipping.Application.Commands.CancelShipment;
using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Shipping.Infrastructure.Messaging;

public sealed class CancelShipmentConsumer(
    ISender sender,
    ILogger<CancelShipmentConsumer> logger)
    : IConsumer<CancelShipment>
{
    public async Task Consume(ConsumeContext<CancelShipment> context)
    {
        ShipmentCancellationResult result = await sender.Send(
            new CancelShipmentCommand(
                new ShipmentCancellationRequest(
                    context.Message.OrderId,
                    context.Message.Reason)),
            context.CancellationToken);

        if (result == ShipmentCancellationResult.NotCancellable)
        {
            logger.LogWarning(
                "Shipment for order {OrderId} could not be cancelled because it has already left the created state.",
                context.Message.OrderId);
        }
    }
}
