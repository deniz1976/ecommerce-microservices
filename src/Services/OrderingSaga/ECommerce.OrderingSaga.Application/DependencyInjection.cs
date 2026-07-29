using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using ECommerce.OrderingSaga.Application.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.OrderingSaga.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingSagaApplication(this IServiceCollection services)
    {
        services.AddScoped<OrderWorkflowService>();
        services.AddScoped<
            ICommandHandler<ProcessWorkflowEventCommand<OrderSubmitted>>,
            ProcessWorkflowEventCommandHandler<OrderSubmitted>>();
        services.AddScoped<
            ICommandHandler<ProcessWorkflowEventCommand<InventoryReserved>>,
            ProcessWorkflowEventCommandHandler<InventoryReserved>>();
        services.AddScoped<
            ICommandHandler<ProcessWorkflowEventCommand<InventoryReservationFailed>>,
            ProcessWorkflowEventCommandHandler<InventoryReservationFailed>>();
        services.AddScoped<
            ICommandHandler<ProcessWorkflowEventCommand<PaymentAuthorized>>,
            ProcessWorkflowEventCommandHandler<PaymentAuthorized>>();
        services.AddScoped<
            ICommandHandler<ProcessWorkflowEventCommand<PaymentFailed>>,
            ProcessWorkflowEventCommandHandler<PaymentFailed>>();
        services.AddScoped<
            ICommandHandler<ProcessWorkflowEventCommand<ShipmentCreated>>,
            ProcessWorkflowEventCommandHandler<ShipmentCreated>>();
        services.AddScoped<
            ICommandHandler<ProcessWorkflowEventCommand<ShipmentFailed>>,
            ProcessWorkflowEventCommandHandler<ShipmentFailed>>();
        return services;
    }
}
