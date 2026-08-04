using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Shipping.Application.Commands.CreateShipment;
using ECommerce.Shipping.Application.Shipments;
using MassTransit;
using MediatR;

namespace ECommerce.Shipping.Infrastructure.Messaging;

public sealed class CreateShipmentConsumer : IConsumer<CreateShipment>
{
    private readonly ISender sender;

    public CreateShipmentConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public async Task Consume(ConsumeContext<CreateShipment> context)
    {
        CreateShipmentResult result = await sender.Send(
            new CreateShipmentCommand(
                new CreateShipmentRequest(
                    context.Message.OrderId,
                    context.Message.CustomerId,
                    context.Message.RecipientName,
                    context.Message.AddressLine,
                    context.Message.City,
                    context.Message.CountryCode,
                    context.Message.PostalCode)),
            context.CancellationToken);

        if (result.Succeeded)
        {
            await context.Publish(new ShipmentCreated(
                Guid.NewGuid(),
                context.Message.CorrelationId,
                context.Message.MessageId,
                DateTimeOffset.UtcNow,
                ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
                context.Message.OrderId,
                context.Message.CustomerId,
                result.ShipmentId ?? Guid.Empty,
                result.TrackingNumber ?? string.Empty), context.CancellationToken);

            return;
        }

        await context.Publish(new ShipmentFailed(
            Guid.NewGuid(),
            context.Message.CorrelationId,
            context.Message.MessageId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            context.Message.OrderId,
            context.Message.CustomerId,
            result.ReasonCode ?? ECommerce.BuildingBlocks.Contracts.Errors.ErrorCodes.ShipmentFailed,
            result.Reason ?? "Shipment failed."), context.CancellationToken);
    }
}
