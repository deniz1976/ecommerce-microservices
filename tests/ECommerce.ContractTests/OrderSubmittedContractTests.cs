using System.Text.Json;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Messaging;
using ECommerce.BuildingBlocks.Contracts.Orders;

namespace ECommerce.ContractTests;

public sealed class OrderSubmittedContractTests
{
    [Fact]
    public void OrderSubmitted_keeps_required_message_metadata()
    {
        Guid messageId = Guid.NewGuid();
        Guid correlationId = Guid.NewGuid();
        Guid causationId = Guid.NewGuid();
        DateTimeOffset occurredAt = DateTimeOffset.UtcNow;

        OrderSubmitted message = new(
            messageId,
            correlationId,
            causationId,
            occurredAt,
            MessageDefaults.CurrentVersion,
            Guid.NewGuid(),
            Guid.NewGuid(),
            25.50m,
            "USD",
            "Test Customer",
            "Address 1",
            "Istanbul",
            "TR",
            "34000",
            [new OrderLine(Guid.NewGuid(), "Test Product", 1, 25.50m, "USD")]);

        Assert.Equal(messageId, message.MessageId);
        Assert.Equal(correlationId, message.CorrelationId);
        Assert.Equal(causationId, message.CausationId);
        Assert.Equal(occurredAt, message.OccurredAt);
        Assert.Equal(MessageDefaults.CurrentVersion, message.Version);
        Assert.Single(message.Items);
    }

    [Fact]
    public void OrderSubmitted_serializes_shipping_address_fields()
    {
        OrderSubmitted message = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            DateTimeOffset.UtcNow,
            MessageDefaults.CurrentVersion,
            Guid.NewGuid(),
            Guid.NewGuid(),
            10m,
            "USD",
            "Test Customer",
            "Address 1",
            "Istanbul",
            "TR",
            "34000",
            [new OrderLine(Guid.NewGuid(), "Test Product", 1, 10m, "USD")]);

        string json = JsonSerializer.Serialize(message);

        Assert.Contains("RecipientName", json);
        Assert.Contains("AddressLine", json);
        Assert.Contains("CountryCode", json);
        Assert.Contains("PostalCode", json);
    }
}
