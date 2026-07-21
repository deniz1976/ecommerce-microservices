using ECommerce.RuntimeChecks.Models;

namespace ECommerce.RuntimeChecks.Clients;

internal interface IGatewayWorkflowClient
{
    Task<UserResponse> RegisterUserAsync(CreateUserRequest request, CancellationToken cancellationToken);

    Task UpsertInventoryAsync(Guid productId, UpsertInventoryRequest request, CancellationToken cancellationToken);

    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);
}
