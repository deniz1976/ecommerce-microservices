namespace ECommerce.BuildingBlocks.Contracts.Messaging;

public interface IMessageContract
{
    Guid MessageId { get; }

    Guid CorrelationId { get; }

    Guid? CausationId { get; }

    DateTimeOffset OccurredAt { get; }

    int Version { get; }
}
