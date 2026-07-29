using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class ProductImageDeletionBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<ProductImageDeletionBackgroundService> logger;

    public ProductImageDeletionBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ProductImageDeletionBackgroundService> logger)
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
                ProductImageDeletionProcessor processor =
                    scope.ServiceProvider.GetRequiredService<ProductImageDeletionProcessor>();
                int processed = await processor.ProcessDueAsync(stoppingToken);
                if (processed > 0)
                {
                    logger.LogInformation(
                        "Processed {Count} durable product image deletion jobs.",
                        processed);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogError(
                    "Durable product image deletion processing failed; the next bounded retry will continue.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
