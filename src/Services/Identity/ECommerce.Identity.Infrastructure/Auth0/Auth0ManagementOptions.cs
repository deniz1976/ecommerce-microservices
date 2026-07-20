namespace ECommerce.Identity.Infrastructure.Auth0;

public sealed class Auth0ManagementOptions
{
    public const string SectionName = "Auth0Management";

    public bool Enabled { get; init; }

    public string Domain { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string CustomerRoleId { get; init; } = string.Empty;

    public string SellerRoleId { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 10;
}
