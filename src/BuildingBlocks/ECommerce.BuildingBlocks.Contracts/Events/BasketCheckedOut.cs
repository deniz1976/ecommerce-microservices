using ECommerce.BuildingBlocks.Contracts.Messaging;
using ECommerce.BuildingBlocks.Contracts.Orders;

namespace ECommerce.BuildingBlocks.Contracts.Events;

public sealed record BasketCheckedOut(
    Guid MessageId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTimeOffset OccurredAt,
    int Version,
    Guid CheckoutId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    string RecipientName,
    string AddressLine,
    string City,
    string CountryCode,
    string PostalCode,
    IReadOnlyCollection<OrderLine> Items) : IEvent;
