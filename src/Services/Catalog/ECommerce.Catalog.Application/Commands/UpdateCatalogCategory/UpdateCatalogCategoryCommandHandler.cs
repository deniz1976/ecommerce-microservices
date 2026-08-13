using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Commands.UpdateCatalogCategory;

public sealed class UpdateCatalogCategoryCommandHandler
    : ICommandHandler<
        UpdateCatalogCategoryCommand,
        Result<ManagedCatalogCategoryResponse>>
{
    private readonly CatalogCategoryManagementService managementService;

    public UpdateCatalogCategoryCommandHandler(
        CatalogCategoryManagementService managementService)
    {
        this.managementService = managementService;
    }

    public Task<Result<ManagedCatalogCategoryResponse>> HandleAsync(
        UpdateCatalogCategoryCommand command,
        CancellationToken cancellationToken)
    {
        return managementService.UpdateAsync(
            command.CategoryId,
            command.Request,
            cancellationToken);
    }
}
