using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Shipping.Application.Commands.CreateShipment;
using ECommerce.Shipping.Application.Queries.GetShipmentByOrderId;
using ECommerce.Shipping.Application.Queries.SearchManagedShipments;
using ECommerce.Shipping.Application.Shipments;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Shipping.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddShippingApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<ShipmentService>();
        services.AddScoped<ShipmentQueryService>();
        services.AddScoped<
            ICommandHandler<CreateShipmentCommand, CreateShipmentResult>,
            CreateShipmentCommandHandler>();
        services.AddScoped<
            IQueryHandler<GetShipmentByOrderIdQuery, Result<ShipmentResponse>>,
            GetShipmentByOrderIdQueryHandler>();
        services.AddScoped<
            IQueryHandler<SearchManagedShipmentsQuery, PagedResult<ShipmentResponse>>,
            SearchManagedShipmentsQueryHandler>();
        return services;
    }
}
