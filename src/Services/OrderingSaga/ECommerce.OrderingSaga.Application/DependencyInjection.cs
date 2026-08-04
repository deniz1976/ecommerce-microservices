using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using ECommerce.OrderingSaga.Application.Workflows;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.OrderingSaga.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingSagaApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<OrderWorkflowService>();
        services.AddScoped<
            IRequestHandler<ProcessWorkflowEventCommand<OrderSubmitted>>,
            ProcessWorkflowEventCommandHandler<OrderSubmitted>>();
        services.AddScoped<
            IRequestHandler<ProcessWorkflowEventCommand<InventoryReserved>>,
            ProcessWorkflowEventCommandHandler<InventoryReserved>>();
        services.AddScoped<
            IRequestHandler<ProcessWorkflowEventCommand<InventoryReservationFailed>>,
            ProcessWorkflowEventCommandHandler<InventoryReservationFailed>>();
        services.AddScoped<
            IRequestHandler<ProcessWorkflowEventCommand<PaymentAuthorized>>,
            ProcessWorkflowEventCommandHandler<PaymentAuthorized>>();
        services.AddScoped<
            IRequestHandler<ProcessWorkflowEventCommand<PaymentFailed>>,
            ProcessWorkflowEventCommandHandler<PaymentFailed>>();
        services.AddScoped<
            IRequestHandler<ProcessWorkflowEventCommand<ShipmentCreated>>,
            ProcessWorkflowEventCommandHandler<ShipmentCreated>>();
        services.AddScoped<
            IRequestHandler<ProcessWorkflowEventCommand<ShipmentFailed>>,
            ProcessWorkflowEventCommandHandler<ShipmentFailed>>();
        services.AddScoped<
            IRequestHandler<ProcessWorkflowEventCommand<OrderCancellationRequested>>,
            ProcessWorkflowEventCommandHandler<OrderCancellationRequested>>();
        return services;
    }
}
