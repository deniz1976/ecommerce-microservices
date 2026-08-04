using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Commands.CreateCatalogBrand;

public sealed class CreateCatalogBrandCommandHandler
    : ICommandHandler<CreateCatalogBrandCommand, Result<CatalogBrandResponse>>
{
    private readonly CatalogReferenceManagementService managementService;

    public CreateCatalogBrandCommandHandler(
        CatalogReferenceManagementService managementService)
    {
        this.managementService = managementService;
    }

    public Task<Result<CatalogBrandResponse>> HandleAsync(
        CreateCatalogBrandCommand command,
        CancellationToken cancellationToken)
    {
        return managementService.CreateBrandAsync(
            command.Request,
            cancellationToken);
    }
}
