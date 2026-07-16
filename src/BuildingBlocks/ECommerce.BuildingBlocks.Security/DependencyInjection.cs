using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace ECommerce.BuildingBlocks.Security;

public static class DependencyInjection
{
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        return services;
    }

    public static IServiceCollection AddOidcReadySecurity(this IServiceCollection services, IConfiguration configuration)
    {
        AuthOptions options = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
        string roleClaimType = string.IsNullOrWhiteSpace(options.RoleClaimType)
            ? AuthOptions.DefaultRoleClaimType
            : options.RoleClaimType.Trim();

        services.AddCurrentUser();

        bool isConfigured = !string.IsNullOrWhiteSpace(options.Authority) && !string.IsNullOrWhiteSpace(options.Audience);

        if (isConfigured)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwt =>
                {
                    jwt.Authority = options.Authority;
                    jwt.Audience = options.Audience;
                    jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
                    jwt.TokenValidationParameters = new TokenValidationParameters
                    {
                        RoleClaimType = roleClaimType
                    };
                });
        }

        services.AddAuthorizationBuilder()
            .AddPolicy(
                AuthorizationPolicies.AuthenticatedUser,
                policy => policy.RequireAuthenticatedUser())
            .AddPolicy(
                AuthorizationPolicies.Admin,
                policy => policy.RequireRole(ApplicationRoles.Admin))
            .AddPolicy(
                AuthorizationPolicies.InventoryWrite,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        context.User.IsInRole(ApplicationRoles.Admin) ||
                        HasPermission(context.User, ApplicationPermissions.InventoryWrite)))
            .AddPolicy(
                AuthorizationPolicies.SellerOrAdmin,
                policy => policy.RequireRole(ApplicationRoles.Seller, ApplicationRoles.Admin))
            .AddPolicy(
                AuthorizationPolicies.CustomerOrAdmin,
                policy => policy.RequireRole(ApplicationRoles.Customer, ApplicationRoles.Admin));

        return services;
    }

    private static bool HasPermission(ClaimsPrincipal user, string permission)
    {
        return user.Claims
            .Where(claim => claim.Type is "permissions" or "scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Contains(permission, StringComparer.Ordinal);
    }
}
