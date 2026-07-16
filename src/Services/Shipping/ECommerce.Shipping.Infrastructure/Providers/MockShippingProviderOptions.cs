namespace ECommerce.Shipping.Infrastructure.Providers;

public sealed class MockShippingProviderOptions
{
    public const string SectionName = "ShippingProvider";

    public string[] RejectedPostalCodes { get; init; } = ["00000"];
}
