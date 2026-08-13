namespace ECommerce.ContractTests;

public sealed class PaymentResponsibilityArchitectureTests
{
    [Fact]
    public void PaymentCommandsAreSplitByResponsibility()
    {
        string directory = RepositoryPath(
            "src", "Services", "Payment",
            "ECommerce.Payment.Application", "Payments");

        Assert.False(File.Exists(Path.Combine(directory, "PaymentService.cs")));
        string authorization = File.ReadAllText(
            Path.Combine(directory, "PaymentAuthorizationService.cs"));
        string refund = File.ReadAllText(
            Path.Combine(directory, "PaymentRefundService.cs"));

        Assert.Contains("AuthorizeAsync", authorization, StringComparison.Ordinal);
        Assert.Contains("CreatePayment", authorization, StringComparison.Ordinal);
        Assert.DoesNotContain("RefundAsync", authorization, StringComparison.Ordinal);
        Assert.DoesNotContain("EvaluateRefund", authorization, StringComparison.Ordinal);

        Assert.Contains("RefundAsync", refund, StringComparison.Ordinal);
        Assert.Contains("EvaluateRefund", refund, StringComparison.Ordinal);
        Assert.DoesNotContain("AuthorizeAsync", refund, StringComparison.Ordinal);
        Assert.DoesNotContain("CreatePayment", refund, StringComparison.Ordinal);
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
