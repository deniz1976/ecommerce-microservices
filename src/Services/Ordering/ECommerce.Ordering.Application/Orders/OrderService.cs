using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed class OrderService
{
    private readonly IOrderRepository repository;
    private readonly IOrderSubmittedPublisher publisher;

    public OrderService(IOrderRepository repository, IOrderSubmittedPublisher publisher)
    {
        this.repository = repository;
        this.publisher = publisher;
    }

    public async Task<Result<OrderResponse>> CreateAsync(CreateOrderRequest request, Guid correlationId, Guid? causationId, CancellationToken cancellationToken)
    {
        if (request.CustomerId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Currency) ||
            string.IsNullOrWhiteSpace(request.RecipientName) ||
            string.IsNullOrWhiteSpace(request.AddressLine) ||
            string.IsNullOrWhiteSpace(request.City) ||
            request.CountryCode.Trim().Length != 2 ||
            string.IsNullOrWhiteSpace(request.PostalCode) ||
            request.Items.Count == 0)
        {
            return Result<OrderResponse>.Failure(new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
        }

        if (request.Items.Any(x => x.ProductId == Guid.Empty || x.Quantity <= 0 || x.UnitPrice < 0 || string.IsNullOrWhiteSpace(x.Currency)))
        {
            return Result<OrderResponse>.Failure(new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
        }

        Order order = new(
            Guid.NewGuid(),
            request.CustomerId,
            request.Currency,
            request.RecipientName.Trim(),
            request.AddressLine.Trim(),
            request.City.Trim(),
            request.CountryCode.Trim().ToUpperInvariant(),
            request.PostalCode.Trim());

        foreach (CreateOrderItemRequest item in request.Items)
        {
            order.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice, item.Currency);
        }

        repository.Add(order);
        await publisher.PublishAsync(order, correlationId, causationId, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<OrderResponse>.Success(order.ToResponse());
    }

    public async Task<Result<OrderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Order? order = await repository.GetByIdAsync(id, cancellationToken);

        return order is null
            ? Result<OrderResponse>.Failure(new Error(ErrorCodes.OrderNotFound, ErrorCodes.OrderNotFound))
            : Result<OrderResponse>.Success(order.ToResponse());
    }

    public async Task<Result<IReadOnlyCollection<OrderResponse>>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Order> orders = await repository.GetByCustomerIdAsync(customerId, cancellationToken);
        return Result<IReadOnlyCollection<OrderResponse>>.Success(orders.Select(x => x.ToResponse()).ToArray());
    }
}
