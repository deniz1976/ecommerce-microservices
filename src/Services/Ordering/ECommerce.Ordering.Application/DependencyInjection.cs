using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Commands.ChangeOrderStatus;
using ECommerce.Ordering.Application.Commands.CreateOrder;
using ECommerce.Ordering.Application.Commands.CreateOrderFromCheckout;
using ECommerce.Ordering.Application.Commands.RequestOrderCancellation;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Application.Queries.GetOrderById;
using ECommerce.Ordering.Application.Queries.GetOrdersByCustomer;
using ECommerce.Ordering.Application.Queries.SearchManagedOrders;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Ordering.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<OrderService>();
        services.AddScoped<OrderStatusService>();
        services.AddScoped<OrderCancellationService>();
        services.AddScoped<OrderQueryService>();
        services.AddScoped<
            ICommandHandler<CreateOrderCommand, Result<OrderResponse>>,
            CreateOrderCommandHandler>();
        services.AddScoped<
            ICommandHandler<CreateOrderFromCheckoutCommand, Result<OrderResponse>>,
            CreateOrderFromCheckoutCommandHandler>();
        services.AddScoped<
            ICommandHandler<ChangeOrderStatusCommand>,
            ChangeOrderStatusCommandHandler>();
        services.AddScoped<
            ICommandHandler<RequestOrderCancellationCommand, Result<OrderResponse>>,
            RequestOrderCancellationCommandHandler>();
        services.AddScoped<
            IQueryHandler<GetOrderByIdQuery, Result<OrderResponse>>,
            GetOrderByIdQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetOrdersByCustomerQuery, Result<PagedResult<OrderSummaryResponse>>>,
            GetOrdersByCustomerQueryHandler>();
        services.AddScoped<
            IQueryHandler<SearchManagedOrdersQuery, Result<PagedResult<OrderSummaryResponse>>>,
            SearchManagedOrdersQueryHandler>();
        return services;
    }
}
