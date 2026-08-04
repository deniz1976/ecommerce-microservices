using MediatR;

namespace ECommerce.BuildingBlocks.Contracts.Cqrs;

public interface IQueryHandler<in TQuery, TResult>
    : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);

    Task<TResult> IRequestHandler<TQuery, TResult>.Handle(
        TQuery request,
        CancellationToken cancellationToken) =>
        HandleAsync(request, cancellationToken);
}
