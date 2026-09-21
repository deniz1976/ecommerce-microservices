using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.EventBus;
using MassTransit;

namespace ECommerce.RuntimeChecks.Clients;

internal sealed class MassTransitWorkflowEventPublisher : IWorkflowEventPublisher
{
    private IBusControl? bus;

    public async Task PublishOrderCancellationRequestedAsync(
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        IBusControl startedBus = await EnsureStartedAsync(cancellationToken);
        await startedBus.Publish(
            new OrderCancellationRequested(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                DateTimeOffset.UtcNow,
                ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
                orderId,
                customerId),
            cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (bus is not null)
        {
            await bus.StopAsync(CancellationToken.None);
        }
    }

    private async Task<IBusControl> EnsureStartedAsync(CancellationToken cancellationToken)
    {
        if (bus is not null)
        {
            return bus;
        }

        RabbitMqOptions options = new RabbitMqOptions
        {
            ConnectionString = Environment.GetEnvironmentVariable("RabbitMq__ConnectionString") ?? string.Empty
        }.Normalize();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException(
                "RabbitMq__ConnectionString must contain the broker connection string used by the runtime services.");
        }

        IBusControl createdBus = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(options.Host, options.Port, options.VirtualHost, host =>
            {
                host.Username(options.Username);
                host.Password(options.Password);

                if (options.UseSsl)
                {
                    host.UseSsl(ssl => { });
                }
            });
        });

        await createdBus.StartAsync(cancellationToken);
        bus = createdBus;
        return createdBus;
    }
}
