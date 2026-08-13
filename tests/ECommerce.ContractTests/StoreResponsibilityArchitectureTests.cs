using ECommerce.Catalog.Application.Commands.CreateStore;
using ECommerce.Catalog.Application.Commands.UpdateStore;
using ECommerce.Catalog.Application.Queries.GetStoreById;
using ECommerce.Catalog.Application.Queries.GetStoresByOwner;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.ContractTests;

public sealed class StoreResponsibilityArchitectureTests
{
    [Theory]
    [InlineData(typeof(CreateStoreCommandHandler), typeof(StoreManagementService))]
    [InlineData(typeof(UpdateStoreCommandHandler), typeof(StoreManagementService))]
    [InlineData(typeof(GetStoreByIdQueryHandler), typeof(StoreQueryService))]
    [InlineData(typeof(GetStoresByOwnerQueryHandler), typeof(StoreQueryService))]
    public void StoreHandlersDependOnTheirFocusedService(Type handlerType, Type serviceType)
    {
        System.Reflection.ConstructorInfo constructor = Assert.Single(handlerType.GetConstructors());
        System.Reflection.ParameterInfo parameter = Assert.Single(constructor.GetParameters());
        Assert.Equal(serviceType, parameter.ParameterType);
    }

    [Fact]
    public void StoreQueriesHaveNoCommitDependency()
    {
        string source = File.ReadAllText(RepositoryPath(
            "src", "Services", "Catalog", "ECommerce.Catalog.Application",
            "Stores", "StoreQueryService.cs"));

        Assert.DoesNotContain("IUnitOfWork", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChangesAsync", source, StringComparison.Ordinal);
    }

    private static string RepositoryPath(params string[] segments)
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            root = Directory.GetParent(root)?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return Path.Combine([root, .. segments]);
    }
}
