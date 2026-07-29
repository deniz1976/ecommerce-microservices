namespace ECommerce.RuntimeChecks.Models;

internal sealed record RuntimeBasketResponse(
    Guid CustomerId,
    string Currency,
    decimal TotalAmount,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<RuntimeBasketItemResponse> Items);
