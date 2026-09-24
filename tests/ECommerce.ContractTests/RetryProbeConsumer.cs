using System.Collections.Concurrent;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ContractTests;

public sealed class RetryProbeConsumer : IConsumer<RetryProbe>
{
    public static ConcurrentDictionary<Guid, int> Attempts { get; } = new();

    public Task Consume(ConsumeContext<RetryProbe> context)
    {
        int attempt = Attempts.AddOrUpdate(context.Message.Id, 1, (_, count) => count + 1);
        if (attempt <= context.Message.FailuresBeforeSuccess)
        {
            throw context.Message.ConcurrencyFailure
                ? new DbUpdateConcurrencyException("Simulated concurrency conflict.")
                : new InvalidOperationException("Simulated failure.");
        }

        return Task.CompletedTask;
    }
}
