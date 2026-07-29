namespace ECommerce.RuntimeChecks.Models;

internal sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Currency,
    RuntimeOrderStatus Status,
    decimal TotalAmount,
    string RecipientName,
    string AddressLine,
    string City,
    string CountryCode,
    string PostalCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<RuntimeOrderItemResponse> Items,
    IReadOnlyCollection<RuntimeOrderStatusHistoryResponse> StatusHistory);
