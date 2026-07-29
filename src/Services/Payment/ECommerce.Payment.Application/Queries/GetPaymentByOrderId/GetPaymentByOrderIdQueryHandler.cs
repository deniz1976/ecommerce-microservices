using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Payment.Application.Payments;

namespace ECommerce.Payment.Application.Queries.GetPaymentByOrderId;

public sealed class GetPaymentByOrderIdQueryHandler
    : IQueryHandler<GetPaymentByOrderIdQuery, Result<PaymentResponse>>
{
    private readonly IRepository<Domain.Payment, Guid> repository;
    private readonly IPaymentIdentityReader identityReader;

    public GetPaymentByOrderIdQueryHandler(
        IRepository<Domain.Payment, Guid> repository,
        IPaymentIdentityReader identityReader)
    {
        this.repository = repository;
        this.identityReader = identityReader;
    }

    public async Task<Result<PaymentResponse>> HandleAsync(
        GetPaymentByOrderIdQuery query,
        CancellationToken cancellationToken)
    {
        Guid? paymentId = await identityReader.FindIdByOrderIdAsync(
            query.OrderId,
            cancellationToken);
        if (paymentId is null)
        {
            return NotFound();
        }

        Domain.Payment? payment = await repository.GetByIdAsync(
            paymentId.Value,
            cancellationToken);
        return payment is null
            ? NotFound()
            : Result<PaymentResponse>.Success(payment.ToResponse());
    }

    private static Result<PaymentResponse> NotFound()
    {
        return Result<PaymentResponse>.Failure(
            new Error(ErrorCodes.PaymentNotFound, ErrorCodes.PaymentNotFound));
    }
}
