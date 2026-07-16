using ECommerce.BuildingBlocks.Contracts.Messaging;
using ECommerce.BuildingBlocks.Contracts.Orders;

namespace ECommerce.BuildingBlocks.Contracts.Events;

public sealed record OrderSubmitted(
    Guid MessageId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTimeOffset OccurredAt,
    int Version,
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    string RecipientName,
    string AddressLine,
    string City,
    string CountryCode,
    string PostalCode,
    IReadOnlyCollection<OrderLine> Items) : IEvent;
