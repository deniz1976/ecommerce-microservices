namespace ECommerce.RuntimeChecks.Models;

internal sealed record RuntimeAddBasketItemRequest(Guid ProductId, int Quantity);
