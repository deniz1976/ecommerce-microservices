using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed class OrderService
{
    private readonly IRepository<Order, Guid> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IOrderReader orderReader;
    private readonly IOrderSubmittedPublisher publisher;

    public OrderService(
        IRepository<Order, Guid> repository,
        IUnitOfWork unitOfWork,
        IOrderReader orderReader,
        IOrderSubmittedPublisher publisher)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
        this.orderReader = orderReader;
        this.publisher = publisher;
    }

    public async Task<Result<OrderResponse>> CreateAsync(CreateOrderRequest request, Guid correlationId, Guid? causationId, CancellationToken cancellationToken)
    {
        return await CreateAsync(Guid.NewGuid(), request, correlationId, causationId, cancellationToken);
    }

    public async Task<Result<OrderResponse>> CreateFromCheckoutAsync(
        BasketCheckedOut checkout,
        CancellationToken cancellationToken)
    {
        Order? existingOrder = await repository.GetByIdAsync(checkout.CheckoutId, cancellationToken);
        if (existingOrder is not null)
        {
            return Result<OrderResponse>.Success(existingOrder.ToResponse());
        }

        CreateOrderRequest request = new(
            checkout.CustomerId,
            checkout.Currency,
            checkout.RecipientName,
            checkout.AddressLine,
            checkout.City,
            checkout.CountryCode,
            checkout.PostalCode,
            checkout.Items.Select(
                item => new CreateOrderItemRequest(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice,
                    item.Currency)).ToArray());

        decimal calculatedTotal = request.Items.Sum(item => item.Quantity * item.UnitPrice);
        if (calculatedTotal != checkout.TotalAmount ||
            request.Items.Any(item => !string.Equals(item.Currency, checkout.Currency, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<OrderResponse>.Failure(new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
        }

        return await CreateAsync(
            checkout.CheckoutId,
            request,
            checkout.CorrelationId,
            checkout.MessageId,
            cancellationToken);
    }

    private async Task<Result<OrderResponse>> CreateAsync(
        Guid orderId,
        CreateOrderRequest request,
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
            return Result<OrderResponse>.Failure(new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
        }

        if (request.Items.Any(x => x.ProductId == Guid.Empty || x.Quantity <= 0 || x.UnitPrice < 0 || string.IsNullOrWhiteSpace(x.Currency)))
        {
            return Result<OrderResponse>.Failure(new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
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
            order.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice, item.Currency);
        }

        repository.Add(order);
        await publisher.PublishAsync(order, correlationId, causationId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
        IReadOnlyCollection<Order> orders = await orderReader.GetByCustomerIdAsync(
            customerId,
            cancellationToken);
        return Result<IReadOnlyCollection<OrderResponse>>.Success(orders.Select(x => x.ToResponse()).ToArray());
    }
}
