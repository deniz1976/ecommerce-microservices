using ECommerce.BuildingBlocks.Contracts.Messaging;

namespace ECommerce.BuildingBlocks.Contracts.Commands;

public sealed record CreateShipment(
    Guid MessageId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTimeOffset OccurredAt,
    int Version,
    Guid OrderId,
    Guid CustomerId,
    string RecipientName,
    string AddressLine,
    string City,
    string CountryCode,
    string PostalCode) : ICommand;
