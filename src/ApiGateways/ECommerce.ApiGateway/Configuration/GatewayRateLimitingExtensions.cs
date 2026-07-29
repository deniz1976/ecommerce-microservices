using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using ECommerce.BuildingBlocks.Contracts.Api;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Localization;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.ApiGateway.Configuration;

public static class GatewayRateLimitingExtensions
{
    public static IServiceCollection AddGatewayRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        GatewayRateLimitingOptions settings = configuration
            .GetSection(GatewayRateLimitingOptions.SectionName)
            .Get<GatewayRateLimitingOptions>() ?? new GatewayRateLimitingOptions();
        Validate(settings);

        services.AddECommerceLocalization();
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                context => CreatePartition(context, settings));
            options.OnRejected = WriteRejectedResponseAsync;
        });
        return services;
    }

    public static IApplicationBuilder UseGatewayRateLimiting(this IApplicationBuilder app) =>
        app.UseRateLimiter();

    private static RateLimitPartition<string> CreatePartition(
        HttpContext context,
        GatewayRateLimitingOptions settings)
    {
        if (!settings.Enabled || IsExempt(context.Request.Path))
        {
            return RateLimitPartition.GetNoLimiter("exempt");
        }

        string remoteAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (HttpMethods.IsPost(context.Request.Method) &&
            context.Request.Path.Equals("/gateway/users", StringComparison.OrdinalIgnoreCase))
        {
            return FixedWindow($"registration:{remoteAddress}", settings.RegistrationPermitLimit, settings.WindowSeconds);
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            string subject = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? remoteAddress;
            return FixedWindow($"authenticated:{subject}", settings.AuthenticatedPermitLimit, settings.WindowSeconds);
        }

        return FixedWindow($"anonymous:{remoteAddress}", settings.AnonymousPermitLimit, settings.WindowSeconds);
    }

    private static RateLimitPartition<string> FixedWindow(string key, int limit, int windowSeconds) =>
        RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = limit,
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(windowSeconds)
            });

    private static bool IsExempt(PathString path) =>
        path.StartsWithSegments("/health") ||
        path.StartsWithSegments("/gateway/health") ||
        path.StartsWithSegments("/gateway/hubs/notifications");

    private static async ValueTask WriteRejectedResponseAsync(
        OnRejectedContext context,
        CancellationToken cancellationToken)
    {
        HttpResponse response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
                .ToString(CultureInfo.InvariantCulture);
        }

        IErrorMessageLocalizer localizer = context.HttpContext.RequestServices
            .GetRequiredService<IErrorMessageLocalizer>();
        ApiErrorResponse body = new(
            context.HttpContext.TraceIdentifier,
            ErrorCodes.RateLimitExceeded,
            localizer.GetMessage(
                ErrorCodes.RateLimitExceeded,
                context.HttpContext.Request.Headers.AcceptLanguage.ToString()),
            null);
        await response.WriteAsJsonAsync(body, cancellationToken);
    }

    private static void Validate(GatewayRateLimitingOptions settings)
    {
        if (settings.WindowSeconds is < 1 or > 3600 ||
            settings.RegistrationPermitLimit is < 1 or > 10000 ||
            settings.AnonymousPermitLimit is < 1 or > 10000 ||
            settings.AuthenticatedPermitLimit is < 1 or > 10000)
        {
            throw new InvalidOperationException(
                $"{GatewayRateLimitingOptions.SectionName} values are outside their supported ranges.");
        }
    }
}
