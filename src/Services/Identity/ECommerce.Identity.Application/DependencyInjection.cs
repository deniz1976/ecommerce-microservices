using ECommerce.Identity.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        return services;
    }
}
