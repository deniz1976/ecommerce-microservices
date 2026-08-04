using MediatR;

namespace ECommerce.BuildingBlocks.Contracts.Cqrs;

public interface ICommandHandler<in TCommand>
    : IRequestHandler<TCommand>
    where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken);

    Task IRequestHandler<TCommand>.Handle(
        TCommand request,
        CancellationToken cancellationToken) =>
        HandleAsync(request, cancellationToken);
}
