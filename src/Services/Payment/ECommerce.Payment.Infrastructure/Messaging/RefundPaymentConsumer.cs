using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.Payment.Application.Payments;
using MassTransit;

namespace ECommerce.Payment.Infrastructure.Messaging;

public sealed class RefundPaymentConsumer : IConsumer<RefundPayment>
{
    private readonly PaymentService paymentService;

    public RefundPaymentConsumer(PaymentService paymentService)
    {
        this.paymentService = paymentService;
    }

    public Task Consume(ConsumeContext<RefundPayment> context)
    {
        return paymentService.RefundAsync(
            new RefundPaymentRequest(
                context.Message.OrderId,
                context.Message.CustomerId,
                context.Message.Amount,
                context.Message.Currency,
                context.Message.Reason),
            context.CancellationToken);
    }
}
