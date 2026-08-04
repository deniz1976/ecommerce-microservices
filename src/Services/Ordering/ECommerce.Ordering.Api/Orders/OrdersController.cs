using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Ordering.Application.Commands.CreateOrder;
using ECommerce.Ordering.Application.Commands.RequestOrderCancellation;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Application.Queries.GetOrderById;
using ECommerce.Ordering.Application.Queries.GetOrdersByCustomer;
using ECommerce.Ordering.Application.Queries.GetSellerOrderById;
using ECommerce.Ordering.Application.Queries.SearchManagedOrders;
using ECommerce.Ordering.Application.Queries.SearchSellerOrders;
using ECommerce.Ordering.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Ordering.Api.Orders;

[ApiController]
[Route("api/v1/orders")]
[Authorize(Policy = AuthorizationPolicies.AuthenticatedUser)]
public sealed class OrdersController(
    ISender sender,
    ICustomerOwnershipAuthorizer ownershipAuthorizer) : ControllerBase
{
    [HttpPost(Name = "CreateOrder")]
    [Authorize(Policy = AuthorizationPolicies.TrustedOrderWrite)]
    public async Task<IResult> CreateAsync(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(request.CustomerId, cancellationToken))
        {
            return OrderResults.Forbidden(HttpContext);
        }

        Guid correlationId = CorrelationReader.Read(HttpContext);
        Guid? causationId = CausationReader.Read(HttpContext);
        Result<OrderResponse> result = await sender.Send(
            new CreateOrderCommand(request, correlationId, causationId),
            cancellationToken);

        return result.IsFailure
            ? OrderResults.FromResult(result, HttpContext)
            : Results.Created($"/api/v1/orders/{result.Value!.Id}", result.Value);
    }

    [HttpGet("{id:guid}", Name = "GetOrderById")]
    public async Task<IResult> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Result<OrderResponse> result = await sender.Send(
            new GetOrderByIdQuery(id),
            cancellationToken);
        if (!result.IsFailure &&
            !await ownershipAuthorizer.CanAccessAsync(result.Value!.CustomerId, cancellationToken))
        {
            result = Result<OrderResponse>.Failure(
                new Error(ErrorCodes.OrderNotFound, ErrorCodes.OrderNotFound));
        }

        return OrderResults.FromResult(result, HttpContext);
    }

    [HttpGet("customer/{customerId:guid}", Name = "GetOrdersByCustomerId")]
    public async Task<IResult> GetByCustomerIdAsync(
        Guid customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return OrderResults.Forbidden(HttpContext);
        }

        Result<PagedResult<OrderSummaryResponse>> result = await sender.Send(
            new GetOrdersByCustomerQuery(
                customerId,
                pageNumber,
                pageSize,
                status,
                sortDescending),
            cancellationToken);
        return OrderResults.FromResult(result, HttpContext);
    }

    [HttpGet("manage", Name = "SearchManagedOrders")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<IResult> SearchManagedAsync(
        [FromQuery] Guid? customerId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        Result<PagedResult<OrderSummaryResponse>> result = await sender.Send(
            new SearchManagedOrdersQuery(
                customerId,
                pageNumber,
                pageSize,
                status,
                sortDescending),
            cancellationToken);
        return OrderResults.FromResult(result, HttpContext);
    }

    [HttpGet("store/{storeId:guid}", Name = "SearchSellerOrders")]
    [Authorize(Policy = AuthorizationPolicies.SellerOrAdmin)]
    public async Task<IResult> SearchSellerAsync(
        Guid storeId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        bool bypassStoreOwnership = User.IsInRole(ApplicationRoles.Admin);
        Result<PagedResult<SellerOrderSummaryResponse>> result = await sender.Send(
            new SearchSellerOrdersQuery(
                storeId,
                pageNumber,
                pageSize,
                status,
                sortDescending,
                new SellerOrderAccessContext(
                    bypassStoreOwnership,
                    bypassStoreOwnership
                        ? null
                        : CustomerAccessTokenReader.Read(HttpContext))),
            cancellationToken);
        return OrderResults.FromResult(result, HttpContext);
    }

    [HttpGet("store/{storeId:guid}/{orderId:guid}", Name = "GetSellerOrderById")]
    [Authorize(Policy = AuthorizationPolicies.SellerOrAdmin)]
    public async Task<IResult> GetSellerOrderByIdAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        bool bypassStoreOwnership = User.IsInRole(ApplicationRoles.Admin);
        Result<SellerOrderDetailResponse> result = await sender.Send(
            new GetSellerOrderByIdQuery(
                storeId,
                orderId,
                new SellerOrderAccessContext(
                    bypassStoreOwnership,
                    bypassStoreOwnership
                        ? null
                        : CustomerAccessTokenReader.Read(HttpContext))),
            cancellationToken);
        return OrderResults.FromResult(result, HttpContext);
    }

    [HttpPut("{id:guid}/cancellation", Name = "RequestOrderCancellation")]
    public async Task<IResult> RequestCancellationAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Result<OrderResponse> existing = await sender.Send(
            new GetOrderByIdQuery(id),
            cancellationToken);
        if (existing.IsFailure ||
            !await ownershipAuthorizer.CanAccessAsync(
                existing.Value!.CustomerId,
                cancellationToken))
        {
            return OrderResults.FromResult(
                Result<OrderResponse>.Failure(
                    new Error(ErrorCodes.OrderNotFound, ErrorCodes.OrderNotFound)),
                HttpContext);
        }

        Result<OrderResponse> result = await sender.Send(
            new RequestOrderCancellationCommand(
                id,
                CorrelationReader.Read(HttpContext),
                CausationReader.Read(HttpContext)),
            cancellationToken);
        return OrderResults.FromResult(result, HttpContext);
    }
}
