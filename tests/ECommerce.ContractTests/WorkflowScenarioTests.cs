using ECommerce.RuntimeChecks.Configuration;

namespace ECommerce.ContractTests;

public sealed class WorkflowScenarioTests
{
    [Theory]
    [InlineData(null, WorkflowScenario.All)]
    [InlineData("all", WorkflowScenario.All)]
    [InlineData("basket-checkout", WorkflowScenario.BasketCheckout)]
    [InlineData("success", WorkflowScenario.Success)]
    [InlineData("inventory-failure", WorkflowScenario.InventoryFailure)]
    [InlineData("payment-failure", WorkflowScenario.PaymentFailure)]
    [InlineData("payment-decline", WorkflowScenario.PaymentDecline)]
    [InlineData("notification-signalr", WorkflowScenario.NotificationSignalR)]
    [InlineData("seller-authorization", WorkflowScenario.SellerAuthorization)]
    [InlineData("shipping-failure", WorkflowScenario.ShippingFailure)]
    [InlineData("customer-cancellation", WorkflowScenario.CustomerCancellation)]
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

    [Fact]
    public void RuntimeTokenRequestsOnlyRequiredPermissions()
    {
        string script = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "scripts",
            "request-runtime-access-token.ps1"));
        string validator = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "scripts",
            "jwt-claim-validation.ps1"));

        Assert.Contains("scope = \"inventory:write customer:act\"", script, StringComparison.Ordinal);
        Assert.Contains("jwt-claim-validation.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Assert-RuntimeAccessTokenClaims", script, StringComparison.Ordinal);
        Assert.Contains(
            "must contain exactly inventory:write and customer:act",
            validator,
            StringComparison.Ordinal);
        Assert.Contains(
            "issuer does not match the expected issuer",
            validator,
            StringComparison.Ordinal);
        Assert.Contains(
            "audience does not include the expected audience",
            validator,
            StringComparison.Ordinal);
        Assert.Contains(
            "expired or has no valid expiration",
            validator,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Admin", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("catalog:write", script, StringComparison.OrdinalIgnoreCase);

        string smokeScript = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "scripts",
            "smoke-test.ps1"));
        Assert.DoesNotContain("Admin role", smokeScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("validated runtime Auth0 access token", smokeScript, StringComparison.Ordinal);
    }

    [Fact]
    public void SellerMediaProbeUsesOnlyProcessTokenAndCleansTemporaryImages()
    {
        string script = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "scripts",
            "check-seller-media.ps1"));
        string launcher = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "scripts",
            "run-seller-media-check.ps1"));

        Assert.Contains("SellerChecks__AccessToken", script, StringComparison.Ordinal);
        Assert.Contains(
            "Add-Type -AssemblyName System.Net.Http",
            script,
            StringComparison.Ordinal);
        Assert.Contains("Assert-JwtCoreClaims", script, StringComparison.Ordinal);
        Assert.Contains("does not contain the Seller role", script, StringComparison.Ordinal);
        Assert.Contains("must not contain the Admin role", script, StringComparison.Ordinal);
        Assert.Contains("/gateway/catalog/stores/mine", script, StringComparison.Ordinal);
        Assert.Contains("/gateway/catalog/manage/products", script, StringComparison.Ordinal);
        Assert.Contains("Invoke-UploadImage", script, StringComparison.Ordinal);
        Assert.Contains("Invoke-SetMainImage", script, StringComparison.Ordinal);
        Assert.Contains("Invoke-DeleteImage", script, StringComparison.Ordinal);
        Assert.Contains("finally", script, StringComparison.Ordinal);
        Assert.DoesNotContain("WriteAllText", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Set-Content", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Write-Host $accessToken", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RuntimeChecks__AccessToken", script, StringComparison.Ordinal);
        Assert.Contains("Read-Host", launcher, StringComparison.Ordinal);
        Assert.Contains("-AsSecureString", launcher, StringComparison.Ordinal);
        Assert.Contains("run-with-secrets.ps1", launcher, StringComparison.Ordinal);
        Assert.Contains("check-seller-media.ps1", launcher, StringComparison.Ordinal);
        Assert.Contains(
            "Remove-Item \"Env:\\SellerChecks__AccessToken\"",
            launcher,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Write-Host $env:SellerChecks", launcher, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ComposeUsesOverridableWindowsSafeHostPorts()
    {
        string repositoryRoot = FindRepositoryRoot();
        string script = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "scripts",
            "run-runtime-container-check.ps1"));
        string compose = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "docker-compose.yml"));

        Assert.Contains("$env:CATALOG_API_HOST_PORT = \"15283\"", script, StringComparison.Ordinal);
        Assert.Contains("$env:ORDERING_API_HOST_PORT = \"15265\"", script, StringComparison.Ordinal);
        Assert.Contains("$env:SHIPPING_API_HOST_PORT = \"15187\"", script, StringComparison.Ordinal);
        Assert.Contains("$env:NOTIFICATION_API_HOST_PORT = \"15234\"", script, StringComparison.Ordinal);
        Assert.Contains("${CATALOG_API_HOST_PORT:-15283}:8080", compose, StringComparison.Ordinal);
        Assert.Contains("${BASKET_API_HOST_PORT:-15041}:8080", compose, StringComparison.Ordinal);
        Assert.Contains("${ORDERING_API_HOST_PORT:-15265}:8080", compose, StringComparison.Ordinal);
        Assert.Contains("${INVENTORY_API_HOST_PORT:-15054}:8080", compose, StringComparison.Ordinal);
        Assert.Contains("${PAYMENT_API_HOST_PORT:-15004}:8080", compose, StringComparison.Ordinal);
        Assert.Contains("${SHIPPING_API_HOST_PORT:-15187}:8080", compose, StringComparison.Ordinal);
        Assert.Contains("${NOTIFICATION_API_HOST_PORT:-15234}:8080", compose, StringComparison.Ordinal);
        Assert.Contains("${IDENTITY_API_HOST_PORT:-15090}:8080", compose, StringComparison.Ordinal);
        Assert.Contains("${ORDERING_SAGA_HOST_PORT:-15274}:8080", compose, StringComparison.Ordinal);
        Assert.Contains("${API_GATEWAY_HOST_PORT:-15080}:8080", compose, StringComparison.Ordinal);
        Assert.Contains("${PROMETHEUS_HOST_PORT:-19090}:9090", compose, StringComparison.Ordinal);
        Assert.Contains(
            "Cors__AllowedOrigins__0: ${CORS_ALLOWED_ORIGIN:-http://localhost:3000}",
            compose,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Auth0ManagementPreflightRequestsOnlyUserUpdatePermission()
    {
        string script = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "scripts",
            "test-auth0-management-access.ps1"));
        string workflow = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            ".github",
            "workflows",
            "runtime-integration.yml"));

        Assert.Contains("scope = \"update:users\"", script, StringComparison.Ordinal);
        Assert.Contains("jwt-claim-validation.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Assert-JwtCoreClaims", script, StringComparison.Ordinal);
        Assert.Contains("exactly the update:users scope", script, StringComparison.Ordinal);
        Assert.DoesNotContain("create:users", script, StringComparison.Ordinal);
        Assert.DoesNotContain("delete:users", script, StringComparison.Ordinal);
        Assert.DoesNotContain("update:clients", script, StringComparison.Ordinal);
        Assert.Contains("./scripts/test-auth0-management-access.ps1", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void DemoPaymentDeclineRuntimeConfigurationIsExplicitAndFailureQueueGuarded()
    {
        string repositoryRoot = FindRepositoryRoot();
        string compose = File.ReadAllText(Path.Combine(repositoryRoot, "docker-compose.yml"));
        string workflow = File.ReadAllText(Path.Combine(
            repositoryRoot,
            ".github",
            "workflows",
            "runtime-integration.yml"));

        Assert.Contains(
            "DemoPayment__Scenario: ${DemoPayment__Scenario:-Success}",
            compose,
            StringComparison.Ordinal);
        Assert.Contains(
            "DemoPayment__AuthorizationDelayMilliseconds: ${DemoPayment__AuthorizationDelayMilliseconds:-0}",
            compose,
            StringComparison.Ordinal);
        Assert.Contains("- payment-decline", workflow, StringComparison.Ordinal);
        Assert.Contains("- customer-cancellation", workflow, StringComparison.Ordinal);
        Assert.Contains(
            "inputs.scenario == 'payment-decline' && 'Decline' || 'Success'",
            workflow,
            StringComparison.Ordinal);
        Assert.Contains(
            "inputs.scenario == 'all' || inputs.scenario == 'customer-cancellation'",
            workflow,
            StringComparison.Ordinal);
        Assert.Contains(
            "Capture RabbitMQ failure queue baseline",
            workflow,
            StringComparison.Ordinal);
        Assert.Contains(
            "Verify RabbitMQ failure queues did not grow",
            workflow,
            StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ECommerce.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Repository root containing ECommerce.sln was not found.");
    }
}
