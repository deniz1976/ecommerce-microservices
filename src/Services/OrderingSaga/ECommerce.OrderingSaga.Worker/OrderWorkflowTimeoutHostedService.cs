using ECommerce.OrderingSaga.Application.Workflows;

namespace ECommerce.OrderingSaga.Worker;

public sealed class OrderWorkflowTimeoutHostedService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<OrderWorkflowTimeoutHostedService> logger;
    private readonly OrderWorkflowTimeoutOptions options;

    public OrderWorkflowTimeoutHostedService(
        IServiceScopeFactory scopeFactory,
        OrderWorkflowTimeoutOptions options,
        ILogger<OrderWorkflowTimeoutHostedService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
        this.options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueWorkflowsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Order workflow timeout polling failed.");
            }

            try
            {
                await Task.Delay(options.PollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    internal async Task ProcessDueWorkflowsAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid> workflowIds;
        using (IServiceScope scope = scopeFactory.CreateScope())
        {
            IOrderWorkflowTimeoutReader reader =
                scope.ServiceProvider.GetRequiredService<IOrderWorkflowTimeoutReader>();
            workflowIds = await reader.FindDueWorkflowIdsAsync(
                DateTimeOffset.UtcNow,
                options.BatchSize,
                cancellationToken);
        }

        foreach (Guid workflowId in workflowIds)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                OrderWorkflowTimeoutService service =
                    scope.ServiceProvider.GetRequiredService<OrderWorkflowTimeoutService>();
                await service.HandleAsync(workflowId, DateTimeOffset.UtcNow, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "A due order workflow timeout could not be processed.");
            }
        }
    }
}
