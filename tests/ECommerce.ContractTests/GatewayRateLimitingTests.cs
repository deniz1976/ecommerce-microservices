using System.Net;
using System.Threading.RateLimiting;
using ECommerce.ApiGateway.Configuration;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.ContractTests;

public sealed class GatewayRateLimitingTests
{
    [Fact]
    public void RegistrationUsesTheConfiguredStrictIpPartition()
    {
        using ServiceProvider provider = CreateProvider(registrationLimit: 2);
        PartitionedRateLimiter<HttpContext> limiter = provider
            .GetRequiredService<IOptions<RateLimiterOptions>>()
            .Value
            .GlobalLimiter!;
        DefaultHttpContext context = Request("/gateway/users", HttpMethods.Post);

        using RateLimitLease first = limiter.AttemptAcquire(context);
        using RateLimitLease second = limiter.AttemptAcquire(context);
        using RateLimitLease rejected = limiter.AttemptAcquire(context);

        Assert.True(first.IsAcquired);
        Assert.True(second.IsAcquired);
        Assert.False(rejected.IsAcquired);
    }

    [Fact]
    public void HealthAndNotificationHubPathsAreNotLimited()
    {
        using ServiceProvider provider = CreateProvider(registrationLimit: 1);
        PartitionedRateLimiter<HttpContext> limiter = provider
            .GetRequiredService<IOptions<RateLimiterOptions>>()
            .Value
            .GlobalLimiter!;

        foreach (string path in new[] { "/health/live", "/gateway/health/catalog", "/gateway/hubs/notifications" })
        {
            using RateLimitLease lease = limiter.AttemptAcquire(Request(path, HttpMethods.Get));
            Assert.True(lease.IsAcquired);
        }
    }

    [Theory]
    [InlineData("en", "Too many requests. Please try again later.")]
    [InlineData("tr", "Çok fazla istek gönderildi. Lütfen daha sonra tekrar deneyin.")]
    public void RateLimitErrorIsLocalized(string culture, string expected)
    {
        ErrorMessageLocalizer localizer = new();

        Assert.Equal(expected, localizer.GetMessage(ErrorCodes.RateLimitExceeded, culture));
    }

    private static ServiceProvider CreateProvider(int registrationLimit)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{GatewayRateLimitingOptions.SectionName}:RegistrationPermitLimit"] = registrationLimit.ToString()
            })
            .Build();
        ServiceCollection services = new();
        services.AddGatewayRateLimiting(configuration);
        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext Request(string path, string method)
    {
        DefaultHttpContext context = new();
        context.Request.Path = path;
        context.Request.Method = method;
        context.Connection.RemoteIpAddress = IPAddress.Loopback;
        return context;
    }
}
