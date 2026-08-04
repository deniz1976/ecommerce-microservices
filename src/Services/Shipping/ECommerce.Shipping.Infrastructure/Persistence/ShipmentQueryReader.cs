using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Shipping.Application.Shipments;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shipping.Infrastructure.Persistence;

public sealed class ShipmentQueryReader : IShipmentQueryReader
{
    private readonly ShippingDbContext dbContext;

    public ShipmentQueryReader(ShippingDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<ShipmentResponse?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return dbContext.Shipments
            .AsNoTracking()
            .Where(shipment => shipment.OrderId == orderId)
            .Select(shipment => new ShipmentResponse(
                shipment.Id,
                shipment.OrderId,
                shipment.CustomerId,
                shipment.TrackingNumber,
                shipment.Status,
                shipment.CreatedAt,
                shipment.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<ShipmentResponse>> SearchAsync(
        ManagedShipmentListCriteria criteria,
        CancellationToken cancellationToken)
    {
        int pageSize = NormalizePageSize(criteria.PageSize);
        IQueryable<Domain.Shipment> shipments = dbContext.Shipments.AsNoTracking();

        if (criteria.CustomerId.HasValue)
        {
            shipments = shipments.Where(shipment =>
                shipment.CustomerId == criteria.CustomerId.Value);
        }

        if (criteria.OrderId.HasValue)
        {
            shipments = shipments.Where(shipment =>
                shipment.OrderId == criteria.OrderId.Value);
        }

        if (criteria.Status.HasValue)
        {
            shipments = shipments.Where(shipment =>
                shipment.Status == criteria.Status.Value);
        }

        if (criteria.CreatedFrom.HasValue)
        {
            DateTimeOffset createdFrom = criteria.CreatedFrom.Value.ToUniversalTime();
            shipments = shipments.Where(shipment => shipment.CreatedAt >= createdFrom);
        }

        if (criteria.CreatedTo.HasValue)
        {
            DateTimeOffset createdTo = criteria.CreatedTo.Value.ToUniversalTime();
            shipments = shipments.Where(shipment => shipment.CreatedAt <= createdTo);
        }

        long totalCount = await shipments.LongCountAsync(cancellationToken);
        int pageNumber = NormalizePageNumber(
            criteria.PageNumber,
            pageSize,
            totalCount);

        IQueryable<ShipmentResponse> projection = shipments.Select(shipment =>
            new ShipmentResponse(
                shipment.Id,
                shipment.OrderId,
                shipment.CustomerId,
                shipment.TrackingNumber,
                shipment.Status,
                shipment.CreatedAt,
                shipment.UpdatedAt));

        projection = ApplyOrdering(
            projection,
            criteria.SortBy,
            criteria.SortDescending);

        ShipmentResponse[] items = await projection
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<ShipmentResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    private static IQueryable<ShipmentResponse> ApplyOrdering(
        IQueryable<ShipmentResponse> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.Trim() switch
        {
            ManagedShipmentSortFields.Status => descending
                ? query.OrderByDescending(item => item.Status).ThenBy(item => item.Id)
                : query.OrderBy(item => item.Status).ThenBy(item => item.Id),
            ManagedShipmentSortFields.UpdatedAt => descending
                ? query.OrderByDescending(item => item.UpdatedAt).ThenBy(item => item.Id)
                : query.OrderBy(item => item.UpdatedAt).ThenBy(item => item.Id),
            _ => descending
                ? query.OrderByDescending(item => item.CreatedAt).ThenBy(item => item.Id)
                : query.OrderBy(item => item.CreatedAt).ThenBy(item => item.Id)
        };
    }

    private static int NormalizePageSize(int pageSize) =>
        pageSize is <= 0 or > ManagedShipmentQueryLimits.MaxPageSize
            ? ManagedShipmentQueryLimits.DefaultPageSize
            : pageSize;

    private static int NormalizePageNumber(
        int pageNumber,
        int pageSize,
        long totalCount)
    {
        long totalPages = Math.Max(
            1,
            (long)Math.Ceiling(totalCount / (double)pageSize));
        return (int)Math.Min(Math.Max(1, pageNumber), totalPages);
    }
}
