using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Shipping.Application.Queries.GetShipmentByOrderId;
using ECommerce.Shipping.Application.Queries.SearchManagedShipments;
using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Shipping.Api.Shipments;

[ApiController]
[Route("api/v1/shipments")]
[Authorize(Policy = AuthorizationPolicies.AuthenticatedUser)]
public sealed class ShipmentsController(
    ISender sender,
    ICustomerOwnershipAuthorizer ownershipAuthorizer) : ControllerBase
{
    [HttpGet("manage", Name = "SearchManagedShipments")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<PagedResult<ShipmentResponse>> SearchManagedAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? orderId = null,
        [FromQuery] ShipmentStatus? status = null,
        [FromQuery] DateTimeOffset? createdFrom = null,
        [FromQuery] DateTimeOffset? createdTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        return await sender.Send(
            new SearchManagedShipmentsQuery(
                new ManagedShipmentListCriteria(
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

    [HttpGet("order/{orderId:guid}", Name = "GetShipmentByOrderId")]
    public async Task<IResult> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        Result<ShipmentResponse> result = await sender.Send(
            new GetShipmentByOrderIdQuery(orderId),
            cancellationToken);
        if (!result.IsFailure &&
            !await ownershipAuthorizer.CanAccessAsync(
                result.Value!.CustomerId,
                cancellationToken))
        {
            result = Result<ShipmentResponse>.Failure(
                new Error(ErrorCodes.ShipmentNotFound, ErrorCodes.ShipmentNotFound));
        }

        return ShippingResults.FromResult(result, HttpContext);
    }
}
