using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.Application.Shipments;

public sealed class ShipmentService
{
    private readonly IRepository<Domain.Shipment, Guid> shipmentRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IShipmentIdentityReader identityReader;
    private readonly IShippingProvider shippingProvider;

    public ShipmentService(
        IRepository<Domain.Shipment, Guid> shipmentRepository,
        IUnitOfWork unitOfWork,
        IShipmentIdentityReader identityReader,
        IShippingProvider shippingProvider)
    {
        this.shipmentRepository = shipmentRepository;
        this.unitOfWork = unitOfWork;
        this.identityReader = identityReader;
        this.shippingProvider = shippingProvider;
    }

    public async Task<CreateShipmentResult> CreateAsync(CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        Guid? existingShipmentId = await identityReader.FindIdByOrderIdAsync(
            request.OrderId,
            cancellationToken);
        Domain.Shipment? existingShipment = existingShipmentId is null
            ? null
            : await shipmentRepository.GetByIdAsync(existingShipmentId.Value, cancellationToken);
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
            await unitOfWork.SaveChangesAsync(cancellationToken);

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
            await unitOfWork.SaveChangesAsync(cancellationToken);

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
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateShipmentResult(true, shipment.Id, shipment.TrackingNumber, null, null);
    }
}
