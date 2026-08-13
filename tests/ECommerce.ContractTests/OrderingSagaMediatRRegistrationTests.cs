using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using ECommerce.OrderingSaga.Application.Workflows;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ContractTests;

public sealed class OrderingSagaMediatRRegistrationTests
{
    [Fact]
    public void CancellationWorkflowHasAFocusedApplicationService()
    {
        Assert.Contains(
            typeof(OrderWorkflowCancellationService).GetMethods(),
            method => method.GetParameters().Any(parameter =>
                parameter.ParameterType == typeof(OrderCancellationRequested)));
    }

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
        Type implementationType = ExpectedHandlerType(typeof(TEvent));

        ServiceDescriptor descriptor = Assert.Single(
            services,
            candidate =>
                candidate.ServiceType == serviceType &&
                candidate.ImplementationType == implementationType);

        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    private static Type ExpectedHandlerType(Type eventType)
    {
        if (eventType == typeof(OrderSubmitted)) return typeof(OrderSubmittedCommandHandler);
        if (eventType == typeof(InventoryReserved)) return typeof(InventoryReservedCommandHandler);
        if (eventType == typeof(InventoryReservationFailed)) return typeof(InventoryReservationFailedCommandHandler);
        if (eventType == typeof(PaymentAuthorized)) return typeof(PaymentAuthorizedCommandHandler);
        if (eventType == typeof(PaymentFailed)) return typeof(PaymentFailedCommandHandler);
        if (eventType == typeof(ShipmentCreated)) return typeof(ShipmentCreatedCommandHandler);
        if (eventType == typeof(ShipmentFailed)) return typeof(ShipmentFailedCommandHandler);
        if (eventType == typeof(OrderCancellationRequested)) return typeof(OrderCancellationRequestedCommandHandler);
        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, "Unsupported workflow event.");
    }
}
