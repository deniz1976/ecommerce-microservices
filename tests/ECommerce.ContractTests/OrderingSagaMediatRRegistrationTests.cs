using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ContractTests;

public sealed class OrderingSagaMediatRRegistrationTests
{
    [Fact]
    public void AddOrderingSagaApplication_RegistersEveryWorkflowEventHandlerForMediatR()
    {
        ServiceCollection services = new();

        services.AddOrderingSagaApplication();

        AssertHandlerRegistration<OrderSubmitted>(services);
        AssertHandlerRegistration<InventoryReserved>(services);
        AssertHandlerRegistration<InventoryReservationFailed>(services);
        AssertHandlerRegistration<PaymentAuthorized>(services);
        AssertHandlerRegistration<PaymentFailed>(services);
        AssertHandlerRegistration<ShipmentCreated>(services);
        AssertHandlerRegistration<ShipmentFailed>(services);
        AssertHandlerRegistration<OrderCancellationRequested>(services);
    }

    private static void AssertHandlerRegistration<TEvent>(IServiceCollection services)
        where TEvent : class
    {
        Type serviceType = typeof(IRequestHandler<ProcessWorkflowEventCommand<TEvent>>);
        Type implementationType = typeof(ProcessWorkflowEventCommandHandler<TEvent>);

        ServiceDescriptor descriptor = Assert.Single(
            services,
            candidate =>
                candidate.ServiceType == serviceType &&
                candidate.ImplementationType == implementationType);

        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}
