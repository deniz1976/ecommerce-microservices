using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Application.Commands.AddBasketItem;
using ECommerce.Basket.Application.Commands.CheckoutBasket;
using ECommerce.Basket.Application.Commands.ClearBasket;
using ECommerce.Basket.Application.Commands.RemoveBasketItem;
using ECommerce.Basket.Application.Queries.GetBasket;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Basket.Api.Baskets;

[ApiController]
[Route("api/v1/baskets")]
[Authorize(Policy = AuthorizationPolicies.AuthenticatedUser)]
public sealed class BasketsController(
    ISender sender,
    ICustomerOwnershipAuthorizer ownershipAuthorizer) : ControllerBase
{
    [HttpGet("{customerId:guid}", Name = "GetBasket")]
    public async Task<IResult> GetAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return BasketResults.Forbidden(HttpContext);
        }

        Result<BasketResponse> result = await sender.Send(
            new GetBasketQuery(customerId),
            cancellationToken);
        return BasketResults.FromResult(result, HttpContext);
    }

    [HttpPut("{customerId:guid}/items", Name = "AddBasketItem")]
    public async Task<IResult> AddItemAsync(
        Guid customerId,
        [FromBody] AddBasketItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return BasketResults.Forbidden(HttpContext);
        }

        Result<BasketResponse> result = await sender.Send(
            new AddBasketItemCommand(customerId, request),
            cancellationToken);
        return BasketResults.FromResult(result, HttpContext);
    }

    [HttpDelete("{customerId:guid}/items/{productId:guid}", Name = "RemoveBasketItem")]
    public async Task<IResult> RemoveItemAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return BasketResults.Forbidden(HttpContext);
        }

        Result<BasketResponse> result = await sender.Send(
            new RemoveBasketItemCommand(customerId, productId),
            cancellationToken);
        return BasketResults.FromResult(result, HttpContext);
    }

    [HttpDelete("{customerId:guid}", Name = "ClearBasket")]
    public async Task<IResult> ClearAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return BasketResults.Forbidden(HttpContext);
        }

        await sender.Send(new ClearBasketCommand(customerId), cancellationToken);
        return Results.NoContent();
    }

    [HttpPost("{customerId:guid}/checkout", Name = "CheckoutBasket")]
    public async Task<IResult> CheckoutAsync(
        Guid customerId,
        [FromBody] CheckoutBasketRequest request,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return BasketResults.Forbidden(HttpContext);
        }

        Result<CheckoutBasketResponse> result = await sender.Send(
            new CheckoutBasketCommand(customerId, request),
            cancellationToken);
        return BasketResults.FromResult(result, HttpContext);
    }
}
