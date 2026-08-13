using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed class OrderCreationService(
    IRepository<Order, Guid> repository,
    IUnitOfWork unitOfWork,
    IOrderSubmittedPublisher publisher)
{
    private static readonly IReadOnlyDictionary<Guid, Guid?> NoStoreAttributions =
        new Dictionary<Guid, Guid?>();

    public Task<Result<OrderResponse>> CreateAsync(
        CreateOrderRequest request,
        Guid correlationId,
        Guid? causationId,
        CancellationToken cancellationToken) =>
        CreateAsync(
            Guid.NewGuid(),
            request,
            NoStoreAttributions,
            correlationId,
            causationId,
            cancellationToken);

    internal async Task<Result<OrderResponse>> CreateAsync(
        Guid orderId,
        CreateOrderRequest request,
        IReadOnlyDictionary<Guid, Guid?> storeAttributions,
        Guid correlationId,
        Guid? causationId,
        CancellationToken cancellationToken)
    {
        if (orderId == Guid.Empty ||
            request.CustomerId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Currency) ||
            string.IsNullOrWhiteSpace(request.RecipientName) ||
            string.IsNullOrWhiteSpace(request.AddressLine) ||
            string.IsNullOrWhiteSpace(request.City) ||
            request.CountryCode.Trim().Length != 2 ||
            string.IsNullOrWhiteSpace(request.PostalCode) ||
            request.Items.Count == 0)
        {
            return ValidationFailure();
        }

        if (request.Items.Any(item =>
                item.ProductId == Guid.Empty ||
                string.IsNullOrWhiteSpace(item.ProductName) ||
                item.Quantity <= 0 ||
                item.UnitPrice < 0 ||
                string.IsNullOrWhiteSpace(item.Currency) ||
                !string.Equals(
                    item.Currency.Trim(),
                    request.Currency.Trim(),
                    StringComparison.OrdinalIgnoreCase)) ||
            request.Items.GroupBy(item => item.ProductId).Any(group => group.Count() > 1) ||
            storeAttributions.Values.Any(storeId => storeId == Guid.Empty))
        {
            return ValidationFailure();
        }

        Order order = new(
            orderId,
            request.CustomerId,
            request.Currency,
            request.RecipientName.Trim(),
            request.AddressLine.Trim(),
            request.City.Trim(),
            request.CountryCode.Trim().ToUpperInvariant(),
            request.PostalCode.Trim());

        foreach (CreateOrderItemRequest item in request.Items)
        {
            storeAttributions.TryGetValue(item.ProductId, out Guid? storeId);
            if (order.AddItem(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice,
                    item.Currency,
                    storeId) != OrderItemMutationResult.Applied)
            {
                return ValidationFailure();
            }
        }

        repository.Add(order);
        await publisher.PublishAsync(order, correlationId, causationId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<OrderResponse>.Success(order.ToResponse());
    }

    private static Result<OrderResponse> ValidationFailure() =>
        Result<OrderResponse>.Failure(
            new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
}
