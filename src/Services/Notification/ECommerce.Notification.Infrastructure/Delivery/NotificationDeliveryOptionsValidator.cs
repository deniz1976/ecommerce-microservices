using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Domain;
using Microsoft.Extensions.Options;

namespace ECommerce.Notification.Infrastructure.Delivery;

public sealed class NotificationDeliveryOptionsValidator(
    IEnumerable<INotificationDeliveryProvider> providers)
    : IValidateOptions<NotificationDeliveryOptions>
{
    public ValidateOptionsResult Validate(string? name, NotificationDeliveryOptions options)
    {
        if (options.PollIntervalSeconds is < 1 or > 300 ||
            options.BatchSize is < 1 or > 100 ||
            options.MaxAttempts is < 1 or > 20)
        {
            return ValidateOptionsResult.Fail(
                "Notification delivery polling, batch, or retry limits are invalid.");
        }

        NotificationDispatchChannel[] channels = options.EnabledChannels.Distinct().ToArray();
        if (channels.Length != options.EnabledChannels.Length ||
            channels.Any(channel => !Enum.IsDefined(channel)))
        {
            return ValidateOptionsResult.Fail("Notification delivery channels must be valid and unique.");
        }

        NotificationDispatchChannel[] providerChannels = providers.Select(provider => provider.Channel).ToArray();
        if (providerChannels.Distinct().Count() != providerChannels.Length)
        {
            return ValidateOptionsResult.Fail("Only one notification provider may own a channel.");
        }

        NotificationDispatchChannel? missingProvider = channels
            .Cast<NotificationDispatchChannel?>()
            .FirstOrDefault(channel => !providerChannels.Contains(channel!.Value));
        return missingProvider.HasValue
            ? ValidateOptionsResult.Fail($"No provider is registered for notification channel {missingProvider.Value}.")
            : ValidateOptionsResult.Success;
    }
}
