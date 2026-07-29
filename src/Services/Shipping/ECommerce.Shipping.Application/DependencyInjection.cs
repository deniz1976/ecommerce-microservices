using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Shipping.Application.Commands.CreateShipment;
using ECommerce.Shipping.Application.Shipments;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Shipping.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddShippingApplication(this IServiceCollection services)
    {
        services.AddScoped<ShipmentService>();
        services.AddScoped<
            ICommandHandler<CreateShipmentCommand, CreateShipmentResult>,
            CreateShipmentCommandHandler>();
        return services;
    }
}
