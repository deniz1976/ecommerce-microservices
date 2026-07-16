namespace ECommerce.BuildingBlocks.Security;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public const string DefaultRoleClaimType = "https://ecommerce.local/claims/roles";

    public string? Authority { get; init; }

    public string? Audience { get; init; }

    public bool RequireHttpsMetadata { get; init; } = true;

    public string RoleClaimType { get; init; } = DefaultRoleClaimType;
}
