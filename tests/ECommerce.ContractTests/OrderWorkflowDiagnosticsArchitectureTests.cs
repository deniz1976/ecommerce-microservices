using ECommerce.BuildingBlocks.Security;
using ECommerce.OrderingSaga.Application.Diagnostics;
using ECommerce.OrderingSaga.Application.Queries.SearchOrderWorkflowDiagnostics;
using ECommerce.OrderingSaga.Worker.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce.ContractTests;

public sealed class OrderWorkflowDiagnosticsArchitectureTests
{
    [Fact]
    public void DiagnosticsControllerRequiresExactAdminPolicy()
    {
        AuthorizeAttribute attribute = Assert.Single(
            typeof(OrderWorkflowDiagnosticsController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.Admin, attribute.Policy);
    }

    [Fact]
    public void DiagnosticsHandlerDependsOnlyOnProjectionReader()
    {
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            typeof(SearchOrderWorkflowDiagnosticsQueryHandler).GetConstructors());
        System.Reflection.ParameterInfo parameter = Assert.Single(constructor.GetParameters());

        Assert.Equal(typeof(OrderWorkflowDiagnosticsService), parameter.ParameterType);
    }

    [Fact]
    public void DiagnosticsResponseExcludesSensitiveWorkflowFields()
    {
        string[] propertyNames = typeof(OrderWorkflowDiagnosticsResponse)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain("CustomerId", propertyNames);
        Assert.DoesNotContain("CorrelationId", propertyNames);
        Assert.DoesNotContain("CancellationReason", propertyNames);
        Assert.DoesNotContain("RecipientName", propertyNames);
        Assert.DoesNotContain("AddressLine", propertyNames);
        Assert.DoesNotContain("TotalAmount", propertyNames);
        Assert.DoesNotContain("Items", propertyNames);
    }

    [Fact]
    public void DiagnosticsReaderIsTrackingFreeAndBounded()
    {
        string path = RepositoryPath(
            "src",
            "Services",
            "OrderingSaga",
            "ECommerce.OrderingSaga.Infrastructure",
            "Persistence",
            "OrderWorkflowDiagnosticsReader.cs");
        string source = File.ReadAllText(path);

        Assert.Contains("AsNoTracking()", source, StringComparison.Ordinal);
        Assert.Contains("MaximumPageSize = 50", source, StringComparison.Ordinal);
        Assert.Contains(".Take(pageSize)", source, StringComparison.Ordinal);
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
