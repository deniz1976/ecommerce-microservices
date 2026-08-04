using MediatR;

namespace ECommerce.BuildingBlocks.Contracts.Cqrs;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
