using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Commands.CreateOrderFromCheckout;
using ECommerce.Ordering.Application.Orders;
using MassTransit;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class BasketCheckedOutConsumer : IConsumer<BasketCheckedOut>
{
    private readonly ICommandHandler<
        CreateOrderFromCheckoutCommand,
        Result<OrderResponse>> commandHandler;

    public BasketCheckedOutConsumer(
        ICommandHandler<CreateOrderFromCheckoutCommand, Result<OrderResponse>> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public async Task Consume(ConsumeContext<BasketCheckedOut> context)
    {
        Result<OrderResponse> result = await commandHandler.HandleAsync(
            new CreateOrderFromCheckoutCommand(context.Message),
            context.CancellationToken);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Trusted checkout {context.Message.CheckoutId} could not create an order. Error code: {result.Error?.Code}.");
        }
    }
}
