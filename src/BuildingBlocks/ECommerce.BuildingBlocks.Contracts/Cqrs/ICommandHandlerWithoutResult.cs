namespace ECommerce.BuildingBlocks.Contracts.Cqrs;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken);
}
