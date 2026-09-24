using ECommerce.BuildingBlocks.EventBus;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ContractTests;

public sealed class MessageRetryPolicyTests
{
    [Fact]
    public async Task ConcurrencyConflictsGetMoreRetriesThanTheGeneralPolicy()
    {
        await using ServiceProvider provider = CreateProvider();
        ITestHarness harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        Guid id = Guid.NewGuid();

        await harness.Bus.Publish(new RetryProbe(id, FailuresBeforeSuccess: 5, ConcurrencyFailure: true));

        Assert.True(await harness.Consumed.Any<RetryProbe>(x => x.Context.Message.Id == id && x.Exception is null));
        Assert.Equal(6, RetryProbeConsumer.Attempts[id]);
    }

    [Fact]
    public async Task OtherFailuresKeepTheGeneralRetryLimit()
    {
        await using ServiceProvider provider = CreateProvider();
        ITestHarness harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        Guid id = Guid.NewGuid();

        await harness.Bus.Publish(new RetryProbe(id, FailuresBeforeSuccess: int.MaxValue, ConcurrencyFailure: false));

        Assert.True(await harness.Published.Any<Fault<RetryProbe>>(x => x.Context.Message.Message.Id == id));
        Assert.Equal(4, RetryProbeConsumer.Attempts[id]);
    }

    private static ServiceProvider CreateProvider() =>
        new ServiceCollection()
            .AddMassTransitTestHarness(configurator =>
            {
                configurator.AddConsumer<RetryProbeConsumer>();
                configurator.UsingInMemory((context, bus) =>
                    bus.ReceiveEndpoint("retry-probe", endpoint =>
                    {
                        endpoint.UseECommerceMessageRetry();
                        endpoint.ConfigureConsumer<RetryProbeConsumer>(context);
                    }));
            })
            .BuildServiceProvider(true);
}
