using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed class CheckoutOrderCreationService(
    IRepository<Order, Guid> repository,
    OrderCreationService orderCreationService)
{
    public async Task<Result<OrderResponse>> CreateAsync(
        BasketCheckedOut checkout,
        CancellationToken cancellationToken)
    {
        Order? existingOrder = await repository.GetByIdAsync(
            checkout.CheckoutId,
            cancellationToken);
        if (existingOrder is not null)
        {
            return Result<OrderResponse>.Success(existingOrder.ToResponse());
        }

        if (checkout.Items.GroupBy(item => item.ProductId).Any(group => group.Count() > 1))
        {
            return ValidationFailure();
        }

        CreateOrderRequest request = new(
            checkout.CustomerId,
            checkout.Currency,
            checkout.RecipientName,
            checkout.AddressLine,
            checkout.City,
            checkout.CountryCode,
            checkout.PostalCode,
            checkout.Items.Select(item => new CreateOrderItemRequest(
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.Currency)).ToArray());

        decimal calculatedTotal = request.Items.Sum(item => item.Quantity * item.UnitPrice);
        if (calculatedTotal != checkout.TotalAmount ||
            request.Items.Any(item => !string.Equals(
                item.Currency,
                checkout.Currency,
                StringComparison.OrdinalIgnoreCase)))
        {
            return ValidationFailure();
        }

        Dictionary<Guid, Guid?> storeAttributions = checkout.Items
            .ToDictionary(item => item.ProductId, item => item.StoreId);
        return await orderCreationService.CreateAsync(
            checkout.CheckoutId,
            request,
            storeAttributions,
            checkout.CorrelationId,
            checkout.MessageId,
            cancellationToken);
    }

    private static Result<OrderResponse> ValidationFailure() =>
        Result<OrderResponse>.Failure(
            new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
}
