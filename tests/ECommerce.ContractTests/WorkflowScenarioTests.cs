using ECommerce.RuntimeChecks.Configuration;

namespace ECommerce.ContractTests;

public sealed class WorkflowScenarioTests
{
    [Theory]
    [InlineData(null, WorkflowScenario.All)]
    [InlineData("all", WorkflowScenario.All)]
    [InlineData("success", WorkflowScenario.Success)]
    [InlineData("inventory-failure", WorkflowScenario.InventoryFailure)]
    [InlineData("payment-failure", WorkflowScenario.PaymentFailure)]
    [InlineData("shipping-failure", WorkflowScenario.ShippingFailure)]
    public void ParseMapsDocumentedScenarioName(string? value, WorkflowScenario expected)
    {
        Assert.Equal(expected, WorkflowScenarios.Parse(value));
    }

    [Fact]
    public void ParseRejectsUnknownScenario()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => WorkflowScenarios.Parse("unknown-failure"));

        Assert.Contains("--scenario", exception.Message, StringComparison.Ordinal);
    }
}
