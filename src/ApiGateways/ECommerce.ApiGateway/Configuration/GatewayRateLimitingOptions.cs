namespace ECommerce.ApiGateway.Configuration;

public sealed class GatewayRateLimitingOptions
{
    public const string SectionName = "GatewayRateLimiting";

    public bool Enabled { get; init; } = true;

    public int WindowSeconds { get; init; } = 60;

    public int RegistrationPermitLimit { get; init; } = 10;

    public int AnonymousPermitLimit { get; init; } = 120;

    public int AuthenticatedPermitLimit { get; init; } = 300;
}
