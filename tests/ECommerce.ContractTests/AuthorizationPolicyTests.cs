using ECommerce.BuildingBlocks.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ECommerce.ContractTests;

public sealed class AuthorizationPolicyTests
{
    [Fact]
    public void JwtValidationUsesConfiguredRoleClaimType()
    {
        const string roleClaimType = "https://example.test/claims/roles";
        Dictionary<string, string?> values = new()
        {
            [$"{AuthOptions.SectionName}:Authority"] = "https://example.test",
            [$"{AuthOptions.SectionName}:Audience"] = "example-api",
            [$"{AuthOptions.SectionName}:RoleClaimType"] = roleClaimType
        };

        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        services.AddOidcReadySecurity(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();
        JwtBearerOptions options = provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.Equal(roleClaimType, options.TokenValidationParameters.RoleClaimType);
    }

    [Theory]
    [InlineData("/hubs/notifications", true)]
    [InlineData("/gateway/hubs/notifications/negotiate", true)]
    [InlineData("/api/v1/orders", false)]
    public async Task QueryStringAccessTokenIsRestrictedToNotificationHubPaths(string path, bool expected)
    {
        Dictionary<string, string?> values = new()
        {
            [$"{AuthOptions.SectionName}:Authority"] = "https://example.test",
            [$"{AuthOptions.SectionName}:Audience"] = "example-api"
        };
        ServiceCollection services = new();
        services.AddOidcReadySecurity(new ConfigurationBuilder().AddInMemoryCollection(values).Build());

        using ServiceProvider provider = services.BuildServiceProvider();
        JwtBearerOptions options = provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);
        DefaultHttpContext httpContext = new();
        httpContext.Request.Path = path;
        httpContext.Request.QueryString = new QueryString("?access_token=query-token");
        MessageReceivedContext context = new(
            httpContext,
            new AuthenticationScheme(JwtBearerDefaults.AuthenticationScheme, null, typeof(JwtBearerHandler)),
            options);

        await options.Events.OnMessageReceived(context);

        Assert.Equal(expected ? "query-token" : null, context.Token);
    }

    [Fact]
    public async Task AdminPolicyRequiresAdminRole()
    {
        AuthorizationPolicy policy = await GetPolicyAsync(AuthorizationPolicies.Admin);

        RolesAuthorizationRequirement requirement = Assert.Single(
            policy.Requirements.OfType<RolesAuthorizationRequirement>());

        Assert.Equal([ApplicationRoles.Admin], requirement.AllowedRoles);
    }

    [Fact]
    public async Task SellerOrAdminPolicyAcceptsBothPrivilegedRoles()
    {
        AuthorizationPolicy policy = await GetPolicyAsync(AuthorizationPolicies.SellerOrAdmin);

        RolesAuthorizationRequirement requirement = Assert.Single(
            policy.Requirements.OfType<RolesAuthorizationRequirement>());

        Assert.Equal(
            [ApplicationRoles.Seller, ApplicationRoles.Admin],
            requirement.AllowedRoles);
    }

    [Theory]
    [InlineData(AuthOptions.DefaultRoleClaimType, ApplicationRoles.Admin)]
    [InlineData("permissions", ApplicationPermissions.InventoryWrite)]
    [InlineData("scope", "openid inventory:write")]
    public async Task InventoryWritePolicyAcceptsAdminOrInventoryPermission(string claimType, string claimValue)
    {
        bool authorized = await AuthorizeInventoryWriteAsync(new Claim(claimType, claimValue));

        Assert.True(authorized);
    }

    [Fact]
    public async Task InventoryWritePolicyRejectsUnrelatedPermission()
    {
        bool authorized = await AuthorizeInventoryWriteAsync(new Claim("permissions", "catalog:write"));

        Assert.False(authorized);
    }

    [Fact]
    public async Task CustomerOrAdminPolicyAcceptsCustomerAndAdminRoles()
    {
        AuthorizationPolicy policy = await GetPolicyAsync(AuthorizationPolicies.CustomerOrAdmin);

        RolesAuthorizationRequirement requirement = Assert.Single(
            policy.Requirements.OfType<RolesAuthorizationRequirement>());

        Assert.Equal(
            [ApplicationRoles.Customer, ApplicationRoles.Admin],
            requirement.AllowedRoles);
    }

    [Fact]
    public async Task FallbackPolicyRequiresAnAuthenticatedUser()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddOidcReadySecurity(new ConfigurationBuilder().Build());

        await using ServiceProvider provider = services.BuildServiceProvider();
        AuthorizationOptions options = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        Assert.NotNull(options.FallbackPolicy);
        Assert.Contains(
            options.FallbackPolicy.Requirements,
            requirement => requirement is DenyAnonymousAuthorizationRequirement);
    }

    private static async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        services.AddOidcReadySecurity(configuration);

        await using ServiceProvider provider = services.BuildServiceProvider();
        AuthorizationOptions options = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        return options.GetPolicy(policyName)
            ?? throw new InvalidOperationException($"Authorization policy '{policyName}' is not registered.");
    }

    private static async Task<bool> AuthorizeInventoryWriteAsync(Claim claim)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddOidcReadySecurity(new ConfigurationBuilder().Build());

        await using ServiceProvider provider = services.BuildServiceProvider();
        IAuthorizationService authorizationService = provider.GetRequiredService<IAuthorizationService>();
        ClaimsIdentity identity = new(
            [claim],
            "Test",
            ClaimTypes.Name,
            AuthOptions.DefaultRoleClaimType);

        AuthorizationResult result = await authorizationService.AuthorizeAsync(
            new ClaimsPrincipal(identity),
            resource: null,
            AuthorizationPolicies.InventoryWrite);

        return result.Succeeded;
    }
}
