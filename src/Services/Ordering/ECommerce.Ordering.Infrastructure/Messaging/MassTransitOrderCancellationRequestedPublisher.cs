using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;
using MassTransit;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class MassTransitOrderCancellationRequestedPublisher(
    IPublishEndpoint publishEndpoint)
    : IOrderCancellationRequestedPublisher
{
    public Task PublishAsync(
        Order order,
        Guid correlationId,
        Guid? causationId,
        CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(
            new OrderCancellationRequested(
                Guid.NewGuid(),
                correlationId,
                causationId,
                DateTimeOffset.UtcNow,
                ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
                order.Id,
                order.CustomerId),
            cancellationToken);
    }
}
