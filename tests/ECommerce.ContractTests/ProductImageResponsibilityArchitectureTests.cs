namespace ECommerce.ContractTests;

public sealed class ProductImageResponsibilityArchitectureTests
{
    [Fact]
    public void ImageOperationsHaveDedicatedCommandsAndServices()
    {
        string root = RepositoryRoot();
        string products = Path.Combine(root, "src", "Services", "Catalog",
            "ECommerce.Catalog.Application", "Products");
        string commands = Path.Combine(root, "src", "Services", "Catalog",
            "ECommerce.Catalog.Application", "Commands", "ManageProductImage");
        Assert.False(File.Exists(Path.Combine(products, "ProductImageService.cs")));
        Assert.False(File.Exists(Path.Combine(commands, "ManageProductImageCommand.cs")));
        foreach (string operation in new[] { "Upload", "SetMain", "Delete" })
        {
            Assert.True(File.Exists(Path.Combine(commands, $"{operation}ProductImageCommand.cs")));
            Assert.True(File.Exists(Path.Combine(commands, $"{operation}ProductImageCommandHandler.cs")));
        }
        Assert.True(File.Exists(Path.Combine(products, "ProductImageUploadService.cs")));
        Assert.True(File.Exists(Path.Combine(products, "ProductImageMainService.cs")));
        Assert.True(File.Exists(Path.Combine(products, "ProductImageDeletionService.cs")));
    }

    private static string RepositoryRoot()
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
            root = Directory.GetParent(root)?.FullName ?? throw new DirectoryNotFoundException();
        return root;
    }
}
