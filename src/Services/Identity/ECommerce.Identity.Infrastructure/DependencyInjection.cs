using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Infrastructure.Persistence;
using ECommerce.Identity.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<IdentityDbContext>(configuration, "IdentityDb");
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        return services;
    }
}
