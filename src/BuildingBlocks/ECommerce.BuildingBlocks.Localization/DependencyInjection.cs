using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BuildingBlocks.Localization;

public static class DependencyInjection
{
    public static IServiceCollection AddECommerceLocalization(this IServiceCollection services)
    {
        services.AddSingleton<IErrorMessageLocalizer, ErrorMessageLocalizer>();
        return services;
    }
}
