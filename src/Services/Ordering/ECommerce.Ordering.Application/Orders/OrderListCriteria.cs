using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed record OrderListCriteria(
    Guid? CustomerId,
    int PageNumber,
    int PageSize,
    OrderStatus? Status,
    bool SortDescending);
