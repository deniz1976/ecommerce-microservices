using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Payment.Application.Payments;
using ECommerce.Payment.Application.Queries.GetPaymentByOrderId;

namespace ECommerce.Payment.Api.Payments;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/payments")
            .WithTags("Payments")
            .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser);

        group.MapGet("/order/{orderId:guid}", GetByOrderIdAsync)
            .WithName("GetPaymentByOrderId");

        return endpoints;
    }

    private static async Task<IResult> GetByOrderIdAsync(
        Guid orderId,
        IQueryHandler<GetPaymentByOrderIdQuery, Result<PaymentResponse>> queryHandler,
        ICustomerOwnershipAuthorizer ownershipAuthorizer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<PaymentResponse> result = await queryHandler.HandleAsync(
            new GetPaymentByOrderIdQuery(orderId),
            cancellationToken);
        if (!result.IsFailure &&
            !await ownershipAuthorizer.CanAccessAsync(result.Value!.CustomerId, cancellationToken))
        {
            result = Result<PaymentResponse>.Failure(
                new Error(ErrorCodes.PaymentNotFound, ErrorCodes.PaymentNotFound));
        }

        return PaymentResults.FromResult(result, httpContext);
    }
}
