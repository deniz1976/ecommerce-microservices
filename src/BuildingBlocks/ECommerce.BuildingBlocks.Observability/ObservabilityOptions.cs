namespace ECommerce.BuildingBlocks.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ServiceNamespace { get; init; } = "ECommerce";

    public string? OtlpEndpoint { get; init; }

    public bool RedactionEnabled { get; init; } = true;
}
