using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Orders;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;
using MassTransit;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class MassTransitOrderSubmittedPublisher : IOrderSubmittedPublisher
{
    private readonly IPublishEndpoint publishEndpoint;

    public MassTransitOrderSubmittedPublisher(IPublishEndpoint publishEndpoint)
    {
        this.publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync(Order order, Guid correlationId, Guid? causationId, CancellationToken cancellationToken)
    {
        OrderSubmitted message = new(
            Guid.NewGuid(),
            correlationId,
            causationId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            order.Currency,
            order.RecipientName,
            order.AddressLine,
            order.City,
            order.CountryCode,
            order.PostalCode,
            order.Items.Select(x => new OrderLine(x.ProductId, x.ProductName, x.Quantity, x.UnitPrice, x.Currency)).ToArray());

        return publishEndpoint.Publish(message, cancellationToken);
    }
}
