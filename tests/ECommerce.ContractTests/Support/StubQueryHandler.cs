using ECommerce.BuildingBlocks.Contracts.Cqrs;

namespace ECommerce.ContractTests.Support;

internal sealed class StubQueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly Func<TQuery, TResponse> handle;

    public StubQueryHandler(Func<TQuery, TResponse> handle)
    {
        this.handle = handle;
    }

    public int InvocationCount { get; private set; }

    public Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken)
    {
        InvocationCount++;
        return Task.FromResult(handle(query));
    }
}
