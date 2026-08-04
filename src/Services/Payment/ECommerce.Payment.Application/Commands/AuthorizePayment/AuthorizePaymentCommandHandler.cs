using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Payment.Application.Payments;

namespace ECommerce.Payment.Application.Commands.AuthorizePayment;

public sealed class AuthorizePaymentCommandHandler
    : ICommandHandler<AuthorizePaymentCommand, PaymentAuthorizationResult>
{
    private readonly PaymentService paymentService;

    public AuthorizePaymentCommandHandler(PaymentService paymentService)
    {
        this.paymentService = paymentService;
    }

    public Task<PaymentAuthorizationResult> HandleAsync(
        AuthorizePaymentCommand command,
        CancellationToken cancellationToken)
    {
        return paymentService.AuthorizeAsync(command.Request, cancellationToken);
    }
}
