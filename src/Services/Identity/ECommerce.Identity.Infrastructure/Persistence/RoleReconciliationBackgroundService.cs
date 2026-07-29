using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Identity.Infrastructure.Persistence;

public sealed class RoleReconciliationBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<RoleReconciliationBackgroundService> logger;

    public RoleReconciliationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<RoleReconciliationBackgroundService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(Interval);
        do
        {
            try
            {
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                RoleReconciliationProcessor processor =
                    scope.ServiceProvider.GetRequiredService<RoleReconciliationProcessor>();
                int processed = await processor.ProcessDueAsync(stoppingToken);
                RoleReconciliationRetentionService retentionService =
                    scope.ServiceProvider.GetRequiredService<RoleReconciliationRetentionService>();
                int deleted = await retentionService.DeleteExpiredCompletedAsync(stoppingToken);
                if (processed > 0)
                {
                    logger.LogInformation(
                        "Processed {Count} durable Auth0 role reconciliation jobs.",
                        processed);
                }

                if (deleted > 0)
                {
                    logger.LogInformation(
                        "Deleted {Count} expired completed Auth0 role reconciliation jobs.",
                        deleted);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogError(
                    "Durable Auth0 role reconciliation processing failed; the next bounded retry will continue.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
