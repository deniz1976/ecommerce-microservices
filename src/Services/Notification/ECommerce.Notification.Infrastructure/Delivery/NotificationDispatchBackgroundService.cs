using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ECommerce.Notification.Infrastructure.Delivery;

public sealed class NotificationDispatchBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<NotificationDeliveryOptions> options)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(
            TimeSpan.FromSeconds(options.Value.PollIntervalSeconds));
        do
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            NotificationDispatchProcessor processor =
                scope.ServiceProvider.GetRequiredService<NotificationDispatchProcessor>();
            await processor.ProcessDueAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
