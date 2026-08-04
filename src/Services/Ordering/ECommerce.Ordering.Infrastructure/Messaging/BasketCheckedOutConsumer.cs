using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Commands.CreateOrderFromCheckout;
using ECommerce.Ordering.Application.Orders;
using MassTransit;
using MediatR;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class BasketCheckedOutConsumer : IConsumer<BasketCheckedOut>
{
    private readonly ISender sender;

    public BasketCheckedOutConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public async Task Consume(ConsumeContext<BasketCheckedOut> context)
    {
        Result<OrderResponse> result = await sender.Send(
            new CreateOrderFromCheckoutCommand(context.Message),
            context.CancellationToken);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Trusted checkout {context.Message.CheckoutId} could not create an order. Error code: {result.Error?.Code}.");
        }
    }
}
