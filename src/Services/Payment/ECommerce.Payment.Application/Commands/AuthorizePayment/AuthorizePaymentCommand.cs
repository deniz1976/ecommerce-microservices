using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Payment.Application.Payments;

namespace ECommerce.Payment.Application.Commands.AuthorizePayment;

public sealed record AuthorizePaymentCommand(PaymentAuthorizationRequest Request)
    : ICommand<PaymentAuthorizationResult>;
