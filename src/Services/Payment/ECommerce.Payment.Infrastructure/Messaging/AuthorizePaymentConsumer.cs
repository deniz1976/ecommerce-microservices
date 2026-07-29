using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Payment.Application.Commands.AuthorizePayment;
using ECommerce.Payment.Application.Payments;
using MassTransit;

namespace ECommerce.Payment.Infrastructure.Messaging;

public sealed class AuthorizePaymentConsumer : IConsumer<AuthorizePayment>
{
    private readonly ICommandHandler<AuthorizePaymentCommand, PaymentAuthorizationResult> commandHandler;

    public AuthorizePaymentConsumer(
        ICommandHandler<AuthorizePaymentCommand, PaymentAuthorizationResult> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public async Task Consume(ConsumeContext<AuthorizePayment> context)
    {
        PaymentAuthorizationResult result = await commandHandler.HandleAsync(
            new AuthorizePaymentCommand(
                new PaymentAuthorizationRequest(
                    context.Message.OrderId,
                    context.Message.CustomerId,
                    context.Message.Amount,
                    context.Message.Currency)),
            context.CancellationToken);

        if (result.Succeeded)
        {
            await context.Publish(new PaymentAuthorized(
                Guid.NewGuid(),
                context.Message.CorrelationId,
                context.Message.MessageId,
                DateTimeOffset.UtcNow,
                ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
                context.Message.OrderId,
                context.Message.CustomerId,
                result.PaymentId ?? Guid.Empty,
                context.Message.Amount,
                context.Message.Currency), context.CancellationToken);

            return;
        }

        await context.Publish(new PaymentFailed(
            Guid.NewGuid(),
            context.Message.CorrelationId,
            context.Message.MessageId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            context.Message.OrderId,
            context.Message.CustomerId,
            result.ReasonCode ?? ECommerce.BuildingBlocks.Contracts.Errors.ErrorCodes.PaymentFailed,
            result.Reason ?? "Payment failed."), context.CancellationToken);
    }
}
