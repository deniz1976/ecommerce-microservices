using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Domain;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Orders;
using MassTransit;

namespace ECommerce.Basket.Infrastructure.Messaging;

public sealed class MassTransitCheckoutPublisher : ICheckoutPublisher
{
    private readonly IPublishEndpoint publishEndpoint;

    public MassTransitCheckoutPublisher(IPublishEndpoint publishEndpoint)
    {
        this.publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync(BasketCheckoutSnapshot snapshot, CancellationToken cancellationToken)
    {
        BasketCheckedOut message = new(
            Guid.NewGuid(),
            snapshot.Id,
            null,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            snapshot.Id,
            snapshot.CustomerId,
            snapshot.TotalAmount,
            snapshot.Currency,
            snapshot.RecipientName,
            snapshot.AddressLine,
            snapshot.City,
            snapshot.CountryCode,
            snapshot.PostalCode,
            snapshot.Items.Select(
                item => new OrderLine(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice,
                    item.Currency,
                    item.StoreId)).ToArray());

        return publishEndpoint.Publish(message, cancellationToken);
    }
}
