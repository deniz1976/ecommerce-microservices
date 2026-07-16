using ECommerce.BuildingBlocks.Contracts.Messaging;

namespace ECommerce.BuildingBlocks.Contracts.Commands;

public sealed record RefundPayment(
    Guid MessageId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTimeOffset OccurredAt,
    int Version,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string Reason) : ICommand;
