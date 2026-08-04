using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Payment.Application.Payments;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Payment.Infrastructure.Persistence;

public sealed class PaymentQueryReader : IPaymentQueryReader
{
    private readonly PaymentDbContext dbContext;

    public PaymentQueryReader(PaymentDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<PaymentResponse?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return dbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.OrderId == orderId)
            .Select(payment => new PaymentResponse(
                payment.Id,
                payment.OrderId,
                payment.CustomerId,
                payment.Amount,
                payment.Currency,
                payment.Status,
                payment.CreatedAt,
                payment.UpdatedAt,
                payment.Transactions
                    .OrderBy(transaction => transaction.CreatedAt)
                    .ThenBy(transaction => transaction.Id)
                    .Select(transaction => new PaymentTransactionResponse(
                        transaction.Id,
                        transaction.Type,
                        transaction.Amount,
                        transaction.Currency,
                        transaction.CreatedAt))
                    .ToArray()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<PaymentSummaryResponse>> SearchAsync(
        ManagedPaymentListCriteria criteria,
        CancellationToken cancellationToken)
    {
        int pageSize = NormalizePageSize(criteria.PageSize);
        IQueryable<Domain.Payment> payments = dbContext.Payments.AsNoTracking();

        if (criteria.CustomerId.HasValue)
        {
            payments = payments.Where(payment =>
                payment.CustomerId == criteria.CustomerId.Value);
        }

        if (criteria.OrderId.HasValue)
        {
            payments = payments.Where(payment =>
                payment.OrderId == criteria.OrderId.Value);
        }

        if (criteria.Status.HasValue)
        {
            payments = payments.Where(payment =>
                payment.Status == criteria.Status.Value);
        }

        if (criteria.CreatedFrom.HasValue)
        {
            DateTimeOffset createdFrom = criteria.CreatedFrom.Value.ToUniversalTime();
            payments = payments.Where(payment => payment.CreatedAt >= createdFrom);
        }

        if (criteria.CreatedTo.HasValue)
        {
            DateTimeOffset createdTo = criteria.CreatedTo.Value.ToUniversalTime();
            payments = payments.Where(payment => payment.CreatedAt <= createdTo);
        }

        long totalCount = await payments.LongCountAsync(cancellationToken);
        int pageNumber = NormalizePageNumber(
            criteria.PageNumber,
            pageSize,
            totalCount);

        IQueryable<PaymentSummaryResponse> projection = payments.Select(payment =>
            new PaymentSummaryResponse(
                payment.Id,
                payment.OrderId,
                payment.CustomerId,
                payment.Amount,
                payment.Currency,
                payment.Status,
                payment.CreatedAt,
                payment.UpdatedAt));

        projection = ApplyOrdering(
            projection,
            criteria.SortBy,
            criteria.SortDescending);

        PaymentSummaryResponse[] items = await projection
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<PaymentSummaryResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    private static IQueryable<PaymentSummaryResponse> ApplyOrdering(
        IQueryable<PaymentSummaryResponse> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.Trim() switch
        {
            ManagedPaymentSortFields.Amount => descending
                ? query.OrderByDescending(item => item.Amount).ThenBy(item => item.Id)
                : query.OrderBy(item => item.Amount).ThenBy(item => item.Id),
            ManagedPaymentSortFields.Status => descending
                ? query.OrderByDescending(item => item.Status).ThenBy(item => item.Id)
                : query.OrderBy(item => item.Status).ThenBy(item => item.Id),
            ManagedPaymentSortFields.UpdatedAt => descending
                ? query.OrderByDescending(item => item.UpdatedAt).ThenBy(item => item.Id)
                : query.OrderBy(item => item.UpdatedAt).ThenBy(item => item.Id),
            _ => descending
                ? query.OrderByDescending(item => item.CreatedAt).ThenBy(item => item.Id)
                : query.OrderBy(item => item.CreatedAt).ThenBy(item => item.Id)
        };
    }

    private static int NormalizePageSize(int pageSize) =>
        pageSize is <= 0 or > ManagedPaymentQueryLimits.MaxPageSize
            ? ManagedPaymentQueryLimits.DefaultPageSize
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
