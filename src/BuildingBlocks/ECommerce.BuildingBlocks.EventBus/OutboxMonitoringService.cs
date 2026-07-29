using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.BuildingBlocks.EventBus;

internal sealed class OutboxMonitoringService<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly OutboxMetrics metrics;
    private readonly ILogger<OutboxMonitoringService<TDbContext>> logger;
    private readonly TimeSpan pollInterval;
    private readonly TimeSpan warningAge;

    public OutboxMonitoringService(
        IServiceScopeFactory scopeFactory,
        OutboxMetrics metrics,
        IOptions<EventBusMonitoringOptions> options,
        ILogger<OutboxMonitoringService<TDbContext>> logger)
    {
        this.scopeFactory = scopeFactory;
        this.metrics = metrics;
        this.logger = logger;
        pollInterval = options.Value.PollInterval;
        warningAge = options.Value.WarningAge;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ObserveAsync(stoppingToken);
            await Task.Delay(pollInterval, stoppingToken);
        }
    }

    internal async Task ObserveAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            IQueryable<OutboxMessage> messages = dbContext.Set<OutboxMessage>().AsNoTracking();
            long messageCount = await messages.LongCountAsync(cancellationToken);
            DateTime? oldestSentTime = await messages
                .Select(message => (DateTime?)message.SentTime)
                .MinAsync(cancellationToken);
            double oldestAgeSeconds = oldestSentTime.HasValue
                ? Math.Max(0, (DateTime.UtcNow - oldestSentTime.Value).TotalSeconds)
                : 0;

            metrics.Update(messageCount, oldestAgeSeconds);

            if (messageCount > 0 && oldestAgeSeconds >= warningAge.TotalSeconds)
            {
                logger.LogWarning(
                    "Transactional outbox backlog detected: {PendingMessageCount} messages, oldest age {OldestMessageAgeSeconds:F0} seconds.",
                    messageCount,
                    oldestAgeSeconds);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            metrics.PollErrors.Add(1);
            logger.LogError(exception, "Transactional outbox monitoring query failed.");
        }
    }
}
