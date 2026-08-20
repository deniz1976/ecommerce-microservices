using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.BuildingBlocks.Contracts.Messaging;

namespace ECommerce.ContractTests;

public sealed class ShipmentStatusUpdateContractTests
{
    [Fact]
    public void UpdateShipmentStatusKeepsRequiredMetadataAndProviderNeutralPayload()
    {
        Guid messageId = Guid.NewGuid();
        Guid correlationId = Guid.NewGuid();
        DateTimeOffset occurredAt = DateTimeOffset.UtcNow;
        UpdateShipmentStatus message = new(
            messageId,
            correlationId,
            null,
            occurredAt,
            MessageDefaults.CurrentVersion,
            "TRACK-123",
            ShipmentProgressStatus.InTransit);

        Assert.Equal(messageId, message.MessageId);
        Assert.Equal(correlationId, message.CorrelationId);
        Assert.Equal(occurredAt, message.OccurredAt);
        Assert.Equal(MessageDefaults.CurrentVersion, message.Version);
        Assert.DoesNotContain(
            message.GetType().GetProperties(),
            property => property.Name.Contains("Provider", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Contains("Reason", StringComparison.OrdinalIgnoreCase));
    }
}
