using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Payment.Application.Payments;

namespace ECommerce.Payment.Application.Queries.SearchManagedPayments;

public sealed record SearchManagedPaymentsQuery(ManagedPaymentListCriteria Criteria)
    : IQuery<PagedResult<PaymentSummaryResponse>>;
