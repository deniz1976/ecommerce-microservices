using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Payment.Application.Payments;

namespace ECommerce.Payment.Application.Commands.RefundPayment;

public sealed record RefundPaymentCommand(RefundPaymentRequest Request) : ICommand;
