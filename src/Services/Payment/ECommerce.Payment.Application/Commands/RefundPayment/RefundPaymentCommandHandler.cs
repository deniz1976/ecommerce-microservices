using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Payment.Application.Payments;

namespace ECommerce.Payment.Application.Commands.RefundPayment;

public sealed class RefundPaymentCommandHandler : ICommandHandler<RefundPaymentCommand>
{
    private readonly PaymentRefundService paymentService;

    public RefundPaymentCommandHandler(PaymentRefundService paymentService)
    {
        this.paymentService = paymentService;
    }

    public Task HandleAsync(
        RefundPaymentCommand command,
        CancellationToken cancellationToken)
    {
        return paymentService.RefundAsync(command.Request, cancellationToken);
    }
}
