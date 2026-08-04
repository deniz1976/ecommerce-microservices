using ECommerce.Ordering.Application.Orders;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Ordering.Infrastructure.Persistence;

public sealed class OrderReader : IOrderReader
{
    private readonly OrderingDbContext dbContext;

    public OrderReader(OrderingDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<PagedResult<OrderSummaryResponse>> SearchAsync(
        OrderListCriteria criteria,
        CancellationToken cancellationToken)
    {
        int pageSize = criteria.PageSize is <= 0 or > OrderQueryLimits.MaxPageSize
            ? OrderQueryLimits.DefaultPageSize
            : criteria.PageSize;
        IQueryable<Order> orders = dbContext.Orders.AsNoTracking();

        if (criteria.CustomerId.HasValue)
        {
            orders = orders.Where(order => order.CustomerId == criteria.CustomerId.Value);
        }

        if (criteria.Status.HasValue)
        {
            orders = orders.Where(order => order.Status == criteria.Status.Value);
        }

        long totalCount = await orders.LongCountAsync(cancellationToken);
        long totalPages = Math.Max(1, (long)Math.Ceiling(totalCount / (double)pageSize));
        int pageNumber = (int)Math.Min(Math.Max(1, criteria.PageNumber), totalPages);

        orders = criteria.SortDescending
            ? orders.OrderByDescending(order => order.CreatedAt).ThenBy(order => order.Id)
            : orders.OrderBy(order => order.CreatedAt).ThenBy(order => order.Id);

        OrderSummaryResponse[] items = await orders
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new OrderSummaryResponse(
                order.Id,
                order.CustomerId,
                order.Currency,
                order.Status,
                order.TotalAmount,
                order.CreatedAt,
                order.UpdatedAt))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<OrderSummaryResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<PagedResult<SellerOrderSummaryResponse>> SearchSellerAsync(
        SellerOrderListCriteria criteria,
        CancellationToken cancellationToken)
    {
        int pageSize = criteria.PageSize is <= 0 or > OrderQueryLimits.MaxPageSize
            ? OrderQueryLimits.DefaultPageSize
            : criteria.PageSize;
        IQueryable<Order> orders = dbContext.Orders
            .AsNoTracking()
            .Where(order => order.Items.Any(item => item.StoreId == criteria.StoreId));

        if (criteria.Status.HasValue)
        {
            orders = orders.Where(order => order.Status == criteria.Status.Value);
        }

        long totalCount = await orders.LongCountAsync(cancellationToken);
        long totalPages = Math.Max(1, (long)Math.Ceiling(totalCount / (double)pageSize));
        int pageNumber = (int)Math.Min(Math.Max(1, criteria.PageNumber), totalPages);

        orders = criteria.SortDescending
            ? orders.OrderByDescending(order => order.CreatedAt).ThenBy(order => order.Id)
            : orders.OrderBy(order => order.CreatedAt).ThenBy(order => order.Id);

        SellerOrderSummaryResponse[] items = await orders
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new SellerOrderSummaryResponse(
                order.Id,
                criteria.StoreId,
                order.Status,
                order.Currency,
                order.Items
                    .Where(item => item.StoreId == criteria.StoreId)
                    .Sum(item => item.Quantity * item.UnitPrice),
                order.CreatedAt,
                order.UpdatedAt,
                order.Items
                    .Where(item => item.StoreId == criteria.StoreId)
                    .OrderBy(item => item.Id)
                    .Select(item => new SellerOrderItemResponse(
                        item.Id,
                        item.ProductId,
                        item.ProductName,
                        item.Quantity,
                        item.UnitPrice,
                        item.Quantity * item.UnitPrice,
                        item.Currency))
                    .ToArray()))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<SellerOrderSummaryResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }
}
