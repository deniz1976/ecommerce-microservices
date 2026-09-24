using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Application.Commands.AddBasketItem;
using ECommerce.Basket.Application.Commands.CheckoutBasket;
using ECommerce.Basket.Application.Commands.ClearBasket;
using ECommerce.Basket.Application.Commands.RemoveBasketItem;
using ECommerce.Basket.Application.Queries.GetBasket;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Basket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBasketApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<BasketQueryService>();
        services.AddScoped<BasketItemAdditionService>();
        services.AddScoped<BasketItemRemovalService>();
        services.AddScoped<BasketClearService>();
        services.AddScoped<BasketCatalogRevalidationService>();
        services.AddScoped<BasketCheckoutService>();
        services.AddScoped<
            IQueryHandler<GetBasketQuery, Result<BasketResponse>>,
            GetBasketQueryHandler>();
        services.AddScoped<
            ICommandHandler<AddBasketItemCommand, Result<BasketResponse>>,
            AddBasketItemCommandHandler>();
        services.AddScoped<
            ICommandHandler<RemoveBasketItemCommand, Result<BasketResponse>>,
            RemoveBasketItemCommandHandler>();
        services.AddScoped<ICommandHandler<ClearBasketCommand, Result>, ClearBasketCommandHandler>();
        services.AddScoped<
            ICommandHandler<CheckoutBasketCommand, Result<CheckoutBasketResponse>>,
            CheckoutBasketCommandHandler>();
        return services;
    }
}
