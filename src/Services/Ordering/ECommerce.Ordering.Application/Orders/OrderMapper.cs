using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public static class OrderMapper
{
    public static OrderResponse ToResponse(this Order order)
    {
        return new OrderResponse(
            order.Id,
            order.CustomerId,
            order.Currency,
            order.Status,
            order.TotalAmount,
            order.RecipientName,
            order.AddressLine,
            order.City,
            order.CountryCode,
            order.PostalCode,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(x => new OrderItemResponse(x.Id, x.ProductId, x.ProductName, x.Quantity, x.UnitPrice, x.TotalPrice, x.Currency)).ToArray(),
            order.StatusHistory
                .OrderBy(x => x.OccurredAt)
                .Select(x => new OrderStatusHistoryResponse(x.Status, x.OccurredAt, x.ReasonCode))
                .ToArray());
    }
}
