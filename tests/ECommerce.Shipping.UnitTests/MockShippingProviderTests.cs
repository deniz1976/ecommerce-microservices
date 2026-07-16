using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Infrastructure.Providers;

namespace ECommerce.Shipping.UnitTests;

public sealed class MockShippingProviderTests
{
    [Fact]
    public async Task CreateAsyncRejectsConfiguredPostalCode()
    {
        MockShippingProviderOptions options = new()
        {
            RejectedPostalCodes = ["00000"]
        };
        MockShippingProvider provider = new(options);
        ShippingProviderRequest request = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Customer",
            "Runtime Avenue 1",
            "Istanbul",
            "TR",
            "00000");

        ShippingProviderResult result = await provider.CreateAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Null(result.TrackingNumber);
        Assert.Contains("00000", result.FailureReason, StringComparison.Ordinal);
    }
}
