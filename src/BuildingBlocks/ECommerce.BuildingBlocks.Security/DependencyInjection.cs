using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.BuildingBlocks.Security;

public static class DependencyInjection
{
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        return services;
    }

    public static IServiceCollection AddCustomerOwnership(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthenticatedUserResolution(configuration);
        services.AddScoped<ICustomerOwnershipAuthorizer, IdentityCustomerOwnershipAuthorizer>();
        return services;
    }

    public static IServiceCollection AddAuthenticatedUserResolution(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        CustomerIdentityOptions options = configuration
            .GetSection(CustomerIdentityOptions.SectionName)
            .Get<CustomerIdentityOptions>() ?? new CustomerIdentityOptions();

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out Uri? baseUri) ||
            (!string.Equals(baseUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"{CustomerIdentityOptions.SectionName}:BaseUrl must be an absolute HTTP or HTTPS URL.");
        }

        if (options.TimeoutSeconds is < 1 or > 30)
        {
            throw new InvalidOperationException(
                $"{CustomerIdentityOptions.SectionName}:TimeoutSeconds must be between 1 and 30.");
        }

        services.AddHttpClient<IAuthenticatedUserResolver, IdentityAuthenticatedUserResolver>(client =>
        {
            client.BaseAddress = baseUri;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        return services;
    }

    public static IServiceCollection AddOidcReadyAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AuthOptions options = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
        string roleClaimType = string.IsNullOrWhiteSpace(options.RoleClaimType)
            ? AuthOptions.DefaultRoleClaimType
            : options.RoleClaimType.Trim();

        bool isConfigured = AuthConfigurationValidator.IsConfigured(options);

        AuthenticationBuilder authentication = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

        if (isConfigured)
        {
            authentication.AddJwtBearer(jwt =>
                {
                    jwt.Authority = options.Authority;
                    jwt.Audience = options.Audience;
                    jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
                    jwt.TokenValidationParameters = new TokenValidationParameters
                    {
                        RoleClaimType = roleClaimType
                    };
                    jwt.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            PathString path = context.HttpContext.Request.Path;
                            if (context.Request.Query.TryGetValue("access_token", out Microsoft.Extensions.Primitives.StringValues token) &&
                                (path.StartsWithSegments("/hubs/notifications") ||
                                 path.StartsWithSegments("/gateway/hubs/notifications")))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
        }
        else
        {
            authentication.AddScheme<AuthenticationSchemeOptions, UnavailableAuthenticationHandler>(
                JwtBearerDefaults.AuthenticationScheme,
                _ => { });
        }

        return services;
    }

    public static IServiceCollection AddOidcReadySecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCurrentUser();
        services.AddOidcReadyAuthentication(configuration);
        services.Replace(ServiceDescriptor.Singleton<
            IAuthorizationMiddlewareResultHandler,
            LocalizedAuthorizationMiddlewareResultHandler>());

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy(
                AuthorizationPolicies.AuthenticatedUser,
                policy => policy.RequireAuthenticatedUser())
            .AddPolicy(
                AuthorizationPolicies.Admin,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(ApplicationRoles.Admin))
            .AddPolicy(
                AuthorizationPolicies.InventoryWrite,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        context.User.IsInRole(ApplicationRoles.Admin) ||
                        ClaimsPrincipalPermissionEvaluator.HasPermission(
                            context.User,
                            ApplicationPermissions.InventoryWrite)))
            .AddPolicy(
                AuthorizationPolicies.InventoryManage,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        context.User.IsInRole(ApplicationRoles.Admin) ||
                        context.User.IsInRole(ApplicationRoles.Seller) ||
                        ClaimsPrincipalPermissionEvaluator.HasPermission(
                            context.User,
                            ApplicationPermissions.InventoryWrite)))
            .AddPolicy(
                AuthorizationPolicies.SellerOrAdmin,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(ApplicationRoles.Seller, ApplicationRoles.Admin))
            .AddPolicy(
                AuthorizationPolicies.CustomerOrAdmin,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(ApplicationRoles.Customer, ApplicationRoles.Admin))
            .AddPolicy(
                AuthorizationPolicies.TrustedOrderWrite,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        context.User.IsInRole(ApplicationRoles.Admin) ||
                        ClaimsPrincipalPermissionEvaluator.HasPermission(
                            context.User,
                            ApplicationPermissions.ActAsCustomer)));

        return services;
    }
}
