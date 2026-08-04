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
    public void AuthenticationOnlyRegistrationDoesNotAddAuthorizationServices()
    {
        ServiceCollection services = new();

        services.AddOidcReadyAuthentication(new ConfigurationBuilder().Build());

        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ServiceType == typeof(IAuthorizationService));
    }

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
    [InlineData("https://example.test", null, true)]
    [InlineData(null, "example-api", true)]
    [InlineData("not-an-authority", "example-api", true)]
    [InlineData("http://example.test", "example-api", true)]
    [InlineData("https://user@example.test", "example-api", true)]
    [InlineData("https://example.test?tenant=unsafe", "example-api", true)]
    public void AuthenticationRegistrationRejectsInvalidOrPartialConfiguration(
        string? authority,
        string? audience,
        bool requireHttpsMetadata)
    {
        IConfiguration configuration = AuthenticationConfiguration(
            authority,
            audience,
            requireHttpsMetadata);
        ServiceCollection services = new();

        Assert.Throws<InvalidOperationException>(
            () => services.AddOidcReadyAuthentication(configuration));
    }

    [Fact]
    public void HttpAuthorityRequiresExplicitlyDisabledHttpsMetadata()
    {
        IConfiguration configuration = AuthenticationConfiguration(
            "http://identity.test",
            "example-api",
            requireHttpsMetadata: false);
        ServiceCollection services = new();

        services.AddOidcReadyAuthentication(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();
        JwtBearerOptions options = provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.Equal("http://identity.test", options.Authority);
        Assert.False(options.RequireHttpsMetadata);
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
    [InlineData("permissions", "inventory:write customer:act")]
    [InlineData("scope", "inventory:write customer:act")]
    public async Task SellerOrAdminPolicyRejectsRuntimeM2MPermissions(
        string claimType,
        string claimValue)
    {
        bool authorized = await AuthorizeAsync(
            AuthorizationPolicies.SellerOrAdmin,
            new Claim(claimType, claimValue));

        Assert.False(authorized);
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

    [Theory]
    [InlineData(AuthOptions.DefaultRoleClaimType, ApplicationRoles.Admin)]
    [InlineData(AuthOptions.DefaultRoleClaimType, ApplicationRoles.Seller)]
    [InlineData("permissions", ApplicationPermissions.InventoryWrite)]
    [InlineData("scope", "openid inventory:write")]
    public async Task InventoryManagePolicyAcceptsAdminSellerOrInventoryPermission(
        string claimType,
        string claimValue)
    {
        bool authorized = await AuthorizeAsync(
            AuthorizationPolicies.InventoryManage,
            new Claim(claimType, claimValue));

        Assert.True(authorized);
    }

    [Theory]
    [InlineData(AuthOptions.DefaultRoleClaimType, ApplicationRoles.Customer)]
    [InlineData("permissions", ApplicationPermissions.ActAsCustomer)]
    public async Task InventoryManagePolicyRejectsCustomerAndUnrelatedPermission(
        string claimType,
        string claimValue)
    {
        bool authorized = await AuthorizeAsync(
            AuthorizationPolicies.InventoryManage,
            new Claim(claimType, claimValue));

        Assert.False(authorized);
    }

    [Theory]
    [InlineData(AuthOptions.DefaultRoleClaimType, ApplicationRoles.Admin, true)]
    [InlineData("permissions", ApplicationPermissions.ActAsCustomer, true)]
    [InlineData("scope", "openid customer:act", true)]
    [InlineData(AuthOptions.DefaultRoleClaimType, ApplicationRoles.Customer, false)]
    [InlineData("permissions", ApplicationPermissions.InventoryWrite, false)]
    public async Task TrustedOrderWriteAcceptsOnlyAdminOrCustomerActor(
        string claimType,
        string claimValue,
        bool expected)
    {
        bool authorized = await AuthorizeAsync(
            AuthorizationPolicies.TrustedOrderWrite,
            new Claim(claimType, claimValue));

        Assert.Equal(expected, authorized);
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

    [Theory]
    [InlineData(AuthorizationPolicies.Admin, ApplicationRoles.Admin)]
    [InlineData(AuthorizationPolicies.SellerOrAdmin, ApplicationRoles.Seller)]
    [InlineData(AuthorizationPolicies.CustomerOrAdmin, ApplicationRoles.Customer)]
    public async Task RolePoliciesRejectRoleClaimsFromUnauthenticatedIdentities(
        string policyName,
        string role)
    {
        bool authorized = await AuthorizeAsync(
            policyName,
            new Claim(AuthOptions.DefaultRoleClaimType, role),
            isAuthenticated: false);

        Assert.False(authorized);
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

    [Fact]
    public async Task EverySharedPolicyExplicitlyRequiresAnAuthenticatedUser()
    {
        string[] policyNames = typeof(AuthorizationPolicies)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(field => field.IsLiteral && !field.IsInitOnly)
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToArray();

        Assert.NotEmpty(policyNames);

        foreach (string policyName in policyNames)
        {
            AuthorizationPolicy policy = await GetPolicyAsync(policyName);

            Assert.Contains(
                policy.Requirements,
                requirement => requirement is DenyAnonymousAuthorizationRequirement);
        }
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
        return await AuthorizeAsync(AuthorizationPolicies.InventoryWrite, claim);
    }

    private static IConfiguration AuthenticationConfiguration(
        string? authority,
        string? audience,
        bool requireHttpsMetadata)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{AuthOptions.SectionName}:Authority"] = authority,
                [$"{AuthOptions.SectionName}:Audience"] = audience,
                [$"{AuthOptions.SectionName}:RequireHttpsMetadata"] = requireHttpsMetadata.ToString()
            })
            .Build();
    }

    private static async Task<bool> AuthorizeAsync(
        string policyName,
        Claim claim,
        bool isAuthenticated = true)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddOidcReadySecurity(new ConfigurationBuilder().Build());

        await using ServiceProvider provider = services.BuildServiceProvider();
        IAuthorizationService authorizationService = provider.GetRequiredService<IAuthorizationService>();
        ClaimsIdentity identity = new(
            [claim],
            isAuthenticated ? "Test" : null,
            ClaimTypes.Name,
            AuthOptions.DefaultRoleClaimType);

        AuthorizationResult result = await authorizationService.AuthorizeAsync(
            new ClaimsPrincipal(identity),
            resource: null,
            policyName);

        return result.Succeeded;
    }
}
