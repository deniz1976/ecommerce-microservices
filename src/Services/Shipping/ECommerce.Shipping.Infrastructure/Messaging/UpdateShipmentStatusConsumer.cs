using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.Shipping.Application.Commands.UpdateShipmentStatus;
using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;
using MassTransit;
using MediatR;

namespace ECommerce.Shipping.Infrastructure.Messaging;

public sealed class UpdateShipmentStatusConsumer(ISender sender)
    : IConsumer<UpdateShipmentStatus>
{
    public Task Consume(ConsumeContext<UpdateShipmentStatus> context)
    {
        return sender.Send(
            new UpdateShipmentStatusCommand(
                new ShipmentStatusUpdateRequest(
                    context.Message.MessageId,
                    context.Message.TrackingNumber,
                    MapStatus(context.Message.Status),
                    context.Message.OccurredAt)),
            context.CancellationToken);
    }

    private static ShipmentStatus MapStatus(ShipmentProgressStatus status)
    {
        return status switch
        {
            ShipmentProgressStatus.InTransit => ShipmentStatus.InTransit,
            ShipmentProgressStatus.Delivered => ShipmentStatus.Delivered,
            ShipmentProgressStatus.Failed => ShipmentStatus.Failed,
            _ => throw new InvalidShipmentStatusUpdateException(
                "Shipment progress status is invalid.")
        };
    }
}
