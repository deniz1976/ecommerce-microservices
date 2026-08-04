namespace ECommerce.ContractTests;

public sealed class FrontendShipmentSummaryTests
{
    [Fact]
    public void CustomerShipmentClientUsesAuthenticatedAbortableOrderRead()
    {
        string api = ReadFrontendFile("lib", "api", "shipping.ts");

        Assert.Contains("getOrderShipment(", api, StringComparison.Ordinal);
        Assert.Contains("`/gateway/shipments/order/${orderId}`", api, StringComparison.Ordinal);
        Assert.Contains("authenticated: true", api, StringComparison.Ordinal);
        Assert.Contains("signal,", api, StringComparison.Ordinal);
    }

    [Fact]
    public void OrderDetailShowsSafeShipmentSummaryOnlyWhenRelevant()
    {
        string detail = ReadFrontendFile("components", "customer", "order-detail.tsx");
        string summary = ReadFrontendFile("components", "customer", "shipment-summary.tsx");

        Assert.Contains("<ShipmentSummary", detail, StringComparison.Ordinal);
        Assert.Contains("shouldShowShipment(state.order)", detail, StringComparison.Ordinal);
        Assert.Contains("entry.reasonCode === \"SHIPMENT_FAILED\"", detail, StringComparison.Ordinal);
        Assert.Contains("error.status === 404 && retryWhenMissing", summary, StringComparison.Ordinal);
        Assert.Contains("new AbortController()", summary, StringComparison.Ordinal);
        Assert.Contains("shipment.trackingNumber", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("address", summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("provider", summary, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadFrontendFile(params string[] segments)
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            root = Directory.GetParent(root)?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return File.ReadAllText(Path.Combine([root, "src", "Frontend", .. segments]));
    }
}
