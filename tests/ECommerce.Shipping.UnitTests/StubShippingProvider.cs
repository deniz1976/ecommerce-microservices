using ECommerce.Shipping.Application.Shipments;

namespace ECommerce.Shipping.UnitTests;

internal sealed class StubShippingProvider : IShippingProvider
{
    private readonly ShippingProviderResult result;

    public StubShippingProvider(ShippingProviderResult result)
    {
        this.result = result;
    }

    public int CallCount { get; private set; }

    public Task<ShippingProviderResult> CreateAsync(
        ShippingProviderRequest request,
        CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(result);
    }
}
