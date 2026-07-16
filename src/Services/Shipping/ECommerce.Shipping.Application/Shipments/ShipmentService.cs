using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.Application.Shipments;

public sealed class ShipmentService
{
    private readonly IShipmentRepository shipmentRepository;
    private readonly IShippingProvider shippingProvider;

    public ShipmentService(IShipmentRepository shipmentRepository, IShippingProvider shippingProvider)
    {
        this.shipmentRepository = shipmentRepository;
        this.shippingProvider = shippingProvider;
    }

    public async Task<CreateShipmentResult> CreateAsync(CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        Domain.Shipment? existingShipment = await shipmentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existingShipment is not null)
        {
            return existingShipment.Status == ShipmentStatus.Failed
                ? new CreateShipmentResult(false, existingShipment.Id, null, ErrorCodes.ShipmentFailed, existingShipment.FailureReason ?? "Shipment failed.")
                : new CreateShipmentResult(true, existingShipment.Id, existingShipment.TrackingNumber, null, null);
        }

        string normalizedRecipientName = request.RecipientName.Trim();
        string normalizedAddressLine = request.AddressLine.Trim();
        string normalizedCity = request.City.Trim();
        string normalizedCountryCode = request.CountryCode.Trim().ToUpperInvariant();
        string normalizedPostalCode = request.PostalCode.Trim();

        if (string.IsNullOrWhiteSpace(normalizedRecipientName) ||
            string.IsNullOrWhiteSpace(normalizedAddressLine) ||
            string.IsNullOrWhiteSpace(normalizedCity) ||
            normalizedCountryCode.Length != 2 ||
            string.IsNullOrWhiteSpace(normalizedPostalCode))
        {
            Domain.Shipment failedShipment = Domain.Shipment.CreateFailed(
                request.OrderId,
                request.CustomerId,
                normalizedRecipientName,
                normalizedAddressLine,
                normalizedCity,
                normalizedCountryCode,
                normalizedPostalCode,
                "Shipment address must be valid.");

            shipmentRepository.Add(failedShipment);
            await shipmentRepository.SaveChangesAsync(cancellationToken);

            return new CreateShipmentResult(false, failedShipment.Id, null, ErrorCodes.ShipmentFailed, failedShipment.FailureReason);
        }

        ShippingProviderResult providerResult = await shippingProvider.CreateAsync(
            new ShippingProviderRequest(
                request.OrderId,
                request.CustomerId,
                normalizedRecipientName,
                normalizedAddressLine,
                normalizedCity,
                normalizedCountryCode,
                normalizedPostalCode),
            cancellationToken);

        if (!providerResult.Succeeded || string.IsNullOrWhiteSpace(providerResult.TrackingNumber))
        {
            string failureReason = providerResult.FailureReason ?? "Shipping provider rejected the shipment.";
            Domain.Shipment failedShipment = Domain.Shipment.CreateFailed(
                request.OrderId,
                request.CustomerId,
                normalizedRecipientName,
                normalizedAddressLine,
                normalizedCity,
                normalizedCountryCode,
                normalizedPostalCode,
                failureReason);

            shipmentRepository.Add(failedShipment);
            await shipmentRepository.SaveChangesAsync(cancellationToken);

            return new CreateShipmentResult(false, failedShipment.Id, null, ErrorCodes.ShipmentFailed, failureReason);
        }

        Domain.Shipment shipment = Domain.Shipment.Create(
            request.OrderId,
            request.CustomerId,
            normalizedRecipientName,
            normalizedAddressLine,
            normalizedCity,
            normalizedCountryCode,
            normalizedPostalCode,
            providerResult.TrackingNumber);

        shipmentRepository.Add(shipment);
        await shipmentRepository.SaveChangesAsync(cancellationToken);

        return new CreateShipmentResult(true, shipment.Id, shipment.TrackingNumber, null, null);
    }
}
