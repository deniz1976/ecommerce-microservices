using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Payment.Application.Payments;
using ECommerce.Payment.Application.Queries.GetPaymentByOrderId;
using ECommerce.Payment.Application.Queries.SearchManagedPayments;
using ECommerce.Payment.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Payment.Api.Payments;

[ApiController]
[Route("api/v1/payments")]
[Authorize(Policy = AuthorizationPolicies.AuthenticatedUser)]
public sealed class PaymentsController(
    ISender sender,
    ICustomerOwnershipAuthorizer ownershipAuthorizer) : ControllerBase
{
    [HttpGet("manage", Name = "SearchManagedPayments")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<PagedResult<PaymentSummaryResponse>> SearchManagedAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? orderId = null,
        [FromQuery] PaymentStatus? status = null,
        [FromQuery] DateTimeOffset? createdFrom = null,
        [FromQuery] DateTimeOffset? createdTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        return await sender.Send(
            new SearchManagedPaymentsQuery(
                new ManagedPaymentListCriteria(
                    pageNumber,
                    pageSize,
                    customerId,
                    orderId,
                    status,
                    createdFrom,
                    createdTo,
                    sortBy,
                    sortDescending)),
            cancellationToken);
    }

    [HttpGet("order/{orderId:guid}", Name = "GetPaymentByOrderId")]
    public async Task<IResult> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        Result<PaymentResponse> result = await sender.Send(
            new GetPaymentByOrderIdQuery(orderId),
            cancellationToken);
        if (!result.IsFailure &&
            !await ownershipAuthorizer.CanAccessAsync(result.Value!.CustomerId, cancellationToken))
        {
            result = Result<PaymentResponse>.Failure(
                new Error(ErrorCodes.PaymentNotFound, ErrorCodes.PaymentNotFound));
        }

        return PaymentResults.FromResult(result, HttpContext);
    }
}
