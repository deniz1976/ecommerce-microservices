namespace ECommerce.ContractTests;

public sealed class SellerOrderVisibilityArchitectureTests
{
    [Fact]
    public void SellerOrderProjectionIsPagedStoreScopedAndOmitsCustomerData()
    {
        string reader = ReadRepositoryFile(
            "src", "Services", "Ordering", "ECommerce.Ordering.Infrastructure",
            "Persistence", "OrderReader.cs");
        string response = ReadRepositoryFile(
            "src", "Services", "Ordering", "ECommerce.Ordering.Application",
            "Orders", "SellerOrderSummaryResponse.cs");
        string detail = ReadRepositoryFile(
            "src", "Services", "Ordering", "ECommerce.Ordering.Application",
            "Orders", "SellerOrderDetailResponse.cs");

        Assert.Contains("order.Items.Any(item => item.StoreId == criteria.StoreId)", reader, StringComparison.Ordinal);
        Assert.Contains(".Where(item => item.StoreId == criteria.StoreId)", reader, StringComparison.Ordinal);
        Assert.Contains("LongCountAsync", reader, StringComparison.Ordinal);
        Assert.Contains(".Skip(", reader, StringComparison.Ordinal);
        Assert.Contains(".Take(pageSize)", reader, StringComparison.Ordinal);
        Assert.Contains("ItemCount", response, StringComparison.Ordinal);
        Assert.DoesNotContain("SellerOrderItemResponse", response, StringComparison.Ordinal);
        Assert.DoesNotContain("CustomerId", response, StringComparison.Ordinal);
        Assert.DoesNotContain("Address", response, StringComparison.Ordinal);
        Assert.DoesNotContain("Recipient", response, StringComparison.Ordinal);
        Assert.Contains("IReadOnlyCollection<SellerOrderItemResponse> Items", detail, StringComparison.Ordinal);
        Assert.DoesNotContain("CustomerId", detail, StringComparison.Ordinal);
        Assert.DoesNotContain("Address", detail, StringComparison.Ordinal);
        Assert.DoesNotContain("Recipient", detail, StringComparison.Ordinal);
    }

    [Fact]
    public void StoreAttributionComesFromCatalogSnapshotAndCheckoutEvent()
    {
        string basketService = ReadRepositoryFile(
            "src", "Services", "Basket", "ECommerce.Basket.Application",
            "Baskets", "BasketMutationService.cs");
        string orderService = ReadRepositoryFile(
            "src", "Services", "Ordering", "ECommerce.Ordering.Application",
            "Orders", "OrderService.cs");
        string directRequest = ReadRepositoryFile(
            "src", "Services", "Ordering", "ECommerce.Ordering.Application",
            "Orders", "CreateOrderItemRequest.cs");

        Assert.Contains("product.StoreId", basketService, StringComparison.Ordinal);
        Assert.Contains("item.StoreId", orderService, StringComparison.Ordinal);
        Assert.DoesNotContain("StoreId", directRequest, StringComparison.Ordinal);
    }

    private static string ReadRepositoryFile(params string[] segments)
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            root = Directory.GetParent(root)?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return File.ReadAllText(Path.Combine([root, .. segments]));
    }
}
