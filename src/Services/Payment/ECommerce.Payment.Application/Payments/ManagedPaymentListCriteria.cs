using ECommerce.Payment.Domain;

namespace ECommerce.Payment.Application.Payments;

public sealed record ManagedPaymentListCriteria(
    int PageNumber,
    int PageSize,
    Guid? CustomerId,
    Guid? OrderId,
    PaymentStatus? Status,
    DateTimeOffset? CreatedFrom,
    DateTimeOffset? CreatedTo,
    string? SortBy,
    bool SortDescending);
