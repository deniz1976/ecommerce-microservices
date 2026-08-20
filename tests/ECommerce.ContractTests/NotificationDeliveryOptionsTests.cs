using ECommerce.Notification.Domain;
using ECommerce.Notification.Infrastructure.Delivery;
using Microsoft.Extensions.Options;

namespace ECommerce.ContractTests;

public sealed class NotificationDeliveryOptionsTests
{
    [Fact]
    public void DisabledExternalChannelsRequireNoProvider()
    {
        NotificationDeliveryOptionsValidator validator = new([]);

        ValidateOptionsResult result = validator.Validate(
            Options.DefaultName,
            new NotificationDeliveryOptions());

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void EnabledChannelWithoutProviderFailsClosed()
    {
        NotificationDeliveryOptionsValidator validator = new([]);

        ValidateOptionsResult result = validator.Validate(
            Options.DefaultName,
            new NotificationDeliveryOptions
            {
                EnabledChannels = [NotificationDispatchChannel.Email]
            });

        Assert.True(result.Failed);
        Assert.Contains("Email", result.FailureMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void EnabledChannelWithSingleProviderIsValid()
    {
        NotificationDeliveryOptionsValidator validator = new(
            [new StubNotificationDeliveryProvider(NotificationDispatchChannel.Push)]);

        ValidateOptionsResult result = validator.Validate(
            Options.DefaultName,
            new NotificationDeliveryOptions
            {
                EnabledChannels = [NotificationDispatchChannel.Push]
            });

        Assert.True(result.Succeeded);
    }
}
