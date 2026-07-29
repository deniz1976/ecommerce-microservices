using ECommerce.BuildingBlocks.Contracts.Cqrs;

namespace ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;

public sealed record ProcessWorkflowEventCommand<TEvent>(TEvent Event) : ICommand
    where TEvent : class;
