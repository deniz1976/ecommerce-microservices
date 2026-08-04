using ECommerce.RuntimeChecks.Models;

namespace ECommerce.RuntimeChecks.Clients;

internal interface IGatewayWorkflowClient
{
    Task<UserResponse> RegisterUserAsync(CreateUserRequest request, CancellationToken cancellationToken);

    Task UpsertInventoryAsync(Guid productId, UpsertInventoryRequest request, CancellationToken cancellationToken);

    Task<RuntimeBasketResponse> AddBasketItemAsync(
        Guid customerId,
        RuntimeAddBasketItemRequest request,
        CancellationToken cancellationToken);

    Task<RuntimeCheckoutBasketResponse> CheckoutBasketAsync(
        Guid customerId,
        RuntimeCheckoutBasketRequest request,
        CancellationToken cancellationToken);

    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);

    Task<OrderResponse> RequestOrderCancellationAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task<OrderResponse> GetOrderAsync(Guid orderId, CancellationToken cancellationToken);
}
