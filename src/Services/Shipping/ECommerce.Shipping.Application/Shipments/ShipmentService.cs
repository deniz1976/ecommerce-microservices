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

        if (!ShipmentAddress.TryCreate(
                request.RecipientName,
                request.AddressLine,
                request.City,
                request.CountryCode,
                request.PostalCode,
                out ShipmentAddress? address))
        {
            Domain.Shipment failedShipment = Domain.Shipment.CreateFailed(
                request.OrderId,
                request.CustomerId,
                request.RecipientName.Trim(),
                request.AddressLine.Trim(),
                request.City.Trim(),
                request.CountryCode.Trim().ToUpperInvariant(),
                request.PostalCode.Trim(),
                "Shipment address must be valid.");

            shipmentRepository.Add(failedShipment);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateShipmentResult(false, failedShipment.Id, null, ErrorCodes.ShipmentFailed, failedShipment.FailureReason);
        }

        ShipmentAddress validAddress = address!;
        ShippingProviderResult providerResult = await shippingProvider.CreateAsync(
            new ShippingProviderRequest(
                request.OrderId,
                request.CustomerId,
                validAddress.RecipientName,
                validAddress.AddressLine,
                validAddress.City,
                validAddress.CountryCode,
                validAddress.PostalCode),
            cancellationToken);

        if (!providerResult.Succeeded || string.IsNullOrWhiteSpace(providerResult.TrackingNumber))
        {
            string failureReason = providerResult.FailureReason ?? "Shipping provider rejected the shipment.";
            Domain.Shipment failedShipment = Domain.Shipment.CreateFailed(
                request.OrderId,
                request.CustomerId,
                validAddress.RecipientName,
                validAddress.AddressLine,
                validAddress.City,
                validAddress.CountryCode,
                validAddress.PostalCode,
                failureReason);

            shipmentRepository.Add(failedShipment);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateShipmentResult(false, failedShipment.Id, null, ErrorCodes.ShipmentFailed, failureReason);
        }

        Domain.Shipment shipment = Domain.Shipment.Create(
            request.OrderId,
            request.CustomerId,
            validAddress,
            providerResult.TrackingNumber);

        shipmentRepository.Add(shipment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateShipmentResult(true, shipment.Id, shipment.TrackingNumber, null, null);
    }
}
