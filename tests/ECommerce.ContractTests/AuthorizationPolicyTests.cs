using ECommerce.BuildingBlocks.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
}
