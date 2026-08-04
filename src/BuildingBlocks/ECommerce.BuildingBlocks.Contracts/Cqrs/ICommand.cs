using MediatR;

namespace ECommerce.BuildingBlocks.Contracts.Cqrs;

public interface ICommand<out TResult> : IRequest<TResult>
{
}
