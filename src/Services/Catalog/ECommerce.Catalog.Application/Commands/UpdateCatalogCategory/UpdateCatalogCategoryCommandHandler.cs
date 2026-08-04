using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Commands.UpdateCatalogCategory;

public sealed class UpdateCatalogCategoryCommandHandler
    : ICommandHandler<
        UpdateCatalogCategoryCommand,
        Result<ManagedCatalogCategoryResponse>>
{
    private readonly CatalogReferenceManagementService managementService;

    public UpdateCatalogCategoryCommandHandler(
        CatalogReferenceManagementService managementService)
    {
        this.managementService = managementService;
    }

    public Task<Result<ManagedCatalogCategoryResponse>> HandleAsync(
        UpdateCatalogCategoryCommand command,
        CancellationToken cancellationToken)
    {
        return managementService.UpdateCategoryAsync(
            command.CategoryId,
            command.Request,
            cancellationToken);
    }
}
