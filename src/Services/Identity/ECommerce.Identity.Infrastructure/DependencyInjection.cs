using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Infrastructure.Auth0;
using ECommerce.Identity.Infrastructure.Persistence;
using ECommerce.Identity.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommerce.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<IdentityDbContext>(configuration, "IdentityDb");
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();

        services.AddOptions<Auth0ManagementOptions>()
            .Bind(configuration.GetSection(Auth0ManagementOptions.SectionName))
            .Validate(
                options => !options.Enabled || IsValidDomain(options.Domain),
                "Auth0Management__Domain must be an HTTPS Auth0 tenant host without a path when synchronization is enabled.")
            .Validate(
                options => !options.Enabled ||
                    (!string.IsNullOrWhiteSpace(options.ClientId) &&
                     !string.IsNullOrWhiteSpace(options.ClientSecret) &&
                     !string.IsNullOrWhiteSpace(options.CustomerRoleId) &&
                     !string.IsNullOrWhiteSpace(options.SellerRoleId)),
                "Auth0 Management API credentials and Customer/Seller role IDs are required when synchronization is enabled.")
            .Validate(options => options.TimeoutSeconds is >= 1 and <= 30,
                "Auth0Management__TimeoutSeconds must be between 1 and 30 seconds.")
            .ValidateOnStart();

        Auth0ManagementOptions managementOptions = configuration
            .GetSection(Auth0ManagementOptions.SectionName)
            .Get<Auth0ManagementOptions>() ?? new Auth0ManagementOptions();

        services.AddHttpClient(Auth0RoleSynchronizer.HttpClientName, client =>
        {
            if (IsValidDomain(managementOptions.Domain))
            {
                client.BaseAddress = new Uri($"https://{managementOptions.Domain}/", UriKind.Absolute);
            }

            client.Timeout = TimeSpan.FromSeconds(managementOptions.TimeoutSeconds);
        });
        services.AddSingleton<IAuth0ManagementTokenProvider, Auth0ManagementTokenProvider>();
        services.Replace(ServiceDescriptor.Singleton<IExternalRoleSynchronizer, Auth0RoleSynchronizer>());

        return services;
    }

    private static bool IsValidDomain(string domain)
    {
        return !string.IsNullOrWhiteSpace(domain) &&
            Uri.TryCreate($"https://{domain}/", UriKind.Absolute, out Uri? uri) &&
            uri.Scheme == Uri.UriSchemeHttps &&
            string.Equals(uri.Host, domain, StringComparison.OrdinalIgnoreCase) &&
            uri.AbsolutePath == "/";
    }
}
