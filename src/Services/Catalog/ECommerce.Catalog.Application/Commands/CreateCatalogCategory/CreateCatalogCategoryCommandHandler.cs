using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Commands.CreateCatalogCategory;

public sealed class CreateCatalogCategoryCommandHandler
    : ICommandHandler<
        CreateCatalogCategoryCommand,
        Result<CatalogCategoryResponse>>
{
    private readonly CatalogCategoryManagementService managementService;

    public CreateCatalogCategoryCommandHandler(
        CatalogCategoryManagementService managementService)
    {
        this.managementService = managementService;
    }

    public Task<Result<CatalogCategoryResponse>> HandleAsync(
        CreateCatalogCategoryCommand command,
        CancellationToken cancellationToken)
    {
        return managementService.CreateAsync(
            command.Request,
            command.Culture,
            cancellationToken);
    }
}
