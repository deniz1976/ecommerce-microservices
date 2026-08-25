namespace ECommerce.ContractTests;

public sealed class ManagedPaymentPagingArchitectureTests
{
    [Fact]
    public void PaymentReaderUsesBoundedNoTrackingSafeProjection()
    {
        string reader = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Payment",
            "ECommerce.Payment.Infrastructure",
            "Persistence",
            "PaymentQueryReader.cs"));

        Assert.Contains("AsNoTracking()", reader, StringComparison.Ordinal);
        Assert.Contains("LongCountAsync", reader, StringComparison.Ordinal);
        Assert.Contains("Select(payment =>", reader, StringComparison.Ordinal);
        Assert.Contains(".Skip(", reader, StringComparison.Ordinal);
        Assert.Contains(".Take(pageSize)", reader, StringComparison.Ordinal);
        Assert.Contains("ThenBy(item => item.Id)", reader, StringComparison.Ordinal);
        Assert.True(
            reader.IndexOf("payments = ApplyOrdering", StringComparison.Ordinal) <
            reader.IndexOf("IQueryable<PaymentSummaryResponse> projection", StringComparison.Ordinal));
        Assert.DoesNotContain("ProviderName", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("ProviderPaymentReference", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("FailureReason", reader, StringComparison.Ordinal);
        Assert.DoesNotContain(".Include(", reader, StringComparison.Ordinal);
    }

    [Fact]
    public void ManagedPaymentQueryIsBoundedAndDoesNotExposeAggregates()
    {
        string limits = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Payment",
            "ECommerce.Payment.Application",
            "Payments",
            "ManagedPaymentQueryLimits.cs"));
        string readerContract = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Payment",
            "ECommerce.Payment.Application",
            "Payments",
            "IPaymentQueryReader.cs"));

        Assert.Contains("MaxPageSize = 100", limits, StringComparison.Ordinal);
        Assert.Contains("PagedResult<PaymentSummaryResponse>", readerContract, StringComparison.Ordinal);
        Assert.DoesNotContain("PagedResult<Payment>", readerContract, StringComparison.Ordinal);
    }

    private static string GetRepositoryPath(params string[] segments)
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            DirectoryInfo? parent = Directory.GetParent(root);
            root = parent?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return Path.Combine([root, .. segments]);
    }
}
