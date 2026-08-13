namespace ECommerce.OrderingSaga.Application.Workflows;

public sealed class OrderWorkflowNotReadyException(Guid orderId)
    : Exception($"The workflow for order '{orderId}' is not available yet.");
