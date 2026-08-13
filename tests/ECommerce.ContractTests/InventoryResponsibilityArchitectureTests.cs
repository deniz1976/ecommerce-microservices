using ECommerce.Inventory.Application.Commands.ReleaseInventory;
using ECommerce.Inventory.Application.Commands.ReserveInventory;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.ContractTests;

public sealed class InventoryResponsibilityArchitectureTests
{
    [Theory]
    [InlineData(typeof(ReserveInventoryCommandHandler), typeof(InventoryReservationService))]
    [InlineData(typeof(ReleaseInventoryCommandHandler), typeof(InventoryReleaseService))]
    public void ReservationHandlersDependOnTheirFocusedService(
        Type handlerType,
        Type serviceType)
    {
        System.Reflection.ConstructorInfo constructor = Assert.Single(handlerType.GetConstructors());
        System.Reflection.ParameterInfo parameter = Assert.Single(constructor.GetParameters());
        Assert.Equal(serviceType, parameter.ParameterType);
    }

    [Fact]
    public void InventoryCommandsAreSplitByResponsibility()
    {
        string directory = RepositoryPath(
            "src", "Services", "Inventory",
            "ECommerce.Inventory.Application", "Inventory");

        Assert.False(File.Exists(Path.Combine(directory, "InventoryService.cs")));
        string reservation = File.ReadAllText(
            Path.Combine(directory, "InventoryReservationService.cs"));
        string release = File.ReadAllText(
            Path.Combine(directory, "InventoryReleaseService.cs"));
        string management = File.ReadAllText(
            Path.Combine(directory, "InventoryManagementService.cs"));

        Assert.Contains("ReserveAsync", reservation, StringComparison.Ordinal);
        Assert.DoesNotContain("ReleaseAsync", reservation, StringComparison.Ordinal);
        Assert.DoesNotContain("UpsertAsync", reservation, StringComparison.Ordinal);
        Assert.DoesNotContain("IProductInventoryAccessAuthorizer", reservation, StringComparison.Ordinal);

        Assert.Contains("ReleaseAsync", release, StringComparison.Ordinal);
        Assert.DoesNotContain("ReserveAsync", release, StringComparison.Ordinal);

        Assert.Contains("UpsertAsync", management, StringComparison.Ordinal);
        Assert.Contains("IProductInventoryAccessAuthorizer", management, StringComparison.Ordinal);
        Assert.DoesNotContain("ReserveAsync", management, StringComparison.Ordinal);
        Assert.DoesNotContain("StockReservation", management, StringComparison.Ordinal);
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
