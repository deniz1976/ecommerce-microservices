using MediatR;

namespace ECommerce.BuildingBlocks.Contracts.Cqrs;

public interface ICommandHandler<in TCommand, TResult>
    : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);

    Task<TResult> IRequestHandler<TCommand, TResult>.Handle(
        TCommand request,
        CancellationToken cancellationToken) =>
        HandleAsync(request, cancellationToken);
}
