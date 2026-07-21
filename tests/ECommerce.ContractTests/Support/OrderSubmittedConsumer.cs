using MassTransit;

namespace ECommerce.ContractTests;

internal sealed class OrderSubmittedConsumer : IConsumer<TestMessage>
{
    public Task Consume(ConsumeContext<TestMessage> context)
    {
        return Task.CompletedTask;
    }
}
