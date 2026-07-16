using ECommerce.Shipping.Application.Shipments;
using Microsoft.Extensions.Options;

namespace ECommerce.Shipping.Infrastructure.Providers;

public sealed class MockShippingProvider : IShippingProvider
{
    private readonly HashSet<string> rejectedPostalCodes;

    public MockShippingProvider(IOptions<MockShippingProviderOptions> options)
        : this(options.Value)
    {
    }

    public MockShippingProvider(MockShippingProviderOptions options)
    {
        rejectedPostalCodes = options.RejectedPostalCodes
            .Where(postalCode => !string.IsNullOrWhiteSpace(postalCode))
            .Select(postalCode => postalCode.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public Task<ShippingProviderResult> CreateAsync(
        ShippingProviderRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (rejectedPostalCodes.Contains(request.PostalCode))
        {
            return Task.FromResult(new ShippingProviderResult(
                false,
                null,
                $"Shipping provider does not serve postal code {request.PostalCode}."));
        }

        string trackingNumber = $"EC-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..24].ToUpperInvariant();
        return Task.FromResult(new ShippingProviderResult(true, trackingNumber, null));
    }
}
