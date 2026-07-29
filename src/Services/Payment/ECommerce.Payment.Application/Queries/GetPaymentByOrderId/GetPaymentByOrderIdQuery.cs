using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Payment.Application.Payments;

namespace ECommerce.Payment.Application.Queries.GetPaymentByOrderId;

public sealed record GetPaymentByOrderIdQuery(Guid OrderId)
    : IQuery<Result<PaymentResponse>>;
