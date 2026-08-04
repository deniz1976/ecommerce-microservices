using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed record SellerOrderListCriteria(
    Guid StoreId,
    int PageNumber,
    int PageSize,
    OrderStatus? Status,
    bool SortDescending);
