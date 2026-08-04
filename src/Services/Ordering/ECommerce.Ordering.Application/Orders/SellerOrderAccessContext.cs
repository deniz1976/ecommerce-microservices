namespace ECommerce.Ordering.Application.Orders;

public sealed record SellerOrderAccessContext(
    bool BypassStoreOwnership,
    string? AccessToken);
