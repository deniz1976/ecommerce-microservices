namespace ECommerce.Shipping.Application.Shipments;

public interface IShippingProvider
{
    Task<ShippingProviderResult> CreateAsync(
        ShippingProviderRequest request,
        CancellationToken cancellationToken);
}
