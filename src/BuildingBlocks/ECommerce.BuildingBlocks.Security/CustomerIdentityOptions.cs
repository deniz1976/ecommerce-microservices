namespace ECommerce.BuildingBlocks.Security;

public sealed class CustomerIdentityOptions
{
    public const string SectionName = "IdentityClient";

    public string BaseUrl { get; init; } = "http://localhost:5090";

    public int TimeoutSeconds { get; init; } = 5;
}
