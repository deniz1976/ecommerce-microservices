using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Payment.Application.Payments;

namespace ECommerce.Payment.Application.Queries.SearchManagedPayments;

public sealed class SearchManagedPaymentsQueryHandler
    : IQueryHandler<SearchManagedPaymentsQuery, PagedResult<PaymentSummaryResponse>>
{
    private readonly PaymentQueryService queryService;

    public SearchManagedPaymentsQueryHandler(PaymentQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<PagedResult<PaymentSummaryResponse>> HandleAsync(
        SearchManagedPaymentsQuery query,
        CancellationToken cancellationToken) =>
        queryService.SearchAsync(query.Criteria, cancellationToken);
}
