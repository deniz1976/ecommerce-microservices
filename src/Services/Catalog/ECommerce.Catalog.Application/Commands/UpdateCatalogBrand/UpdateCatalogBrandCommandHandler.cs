using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Commands.UpdateCatalogBrand;

public sealed class UpdateCatalogBrandCommandHandler
    : ICommandHandler<
        UpdateCatalogBrandCommand,
        Result<ManagedCatalogBrandResponse>>
{
    private readonly CatalogBrandManagementService managementService;

    public UpdateCatalogBrandCommandHandler(
        CatalogBrandManagementService managementService)
    {
        this.managementService = managementService;
    }

    public Task<Result<ManagedCatalogBrandResponse>> HandleAsync(
        UpdateCatalogBrandCommand command,
        CancellationToken cancellationToken)
    {
        return managementService.UpdateAsync(
            command.BrandId,
            command.Request,
            cancellationToken);
    }
}
