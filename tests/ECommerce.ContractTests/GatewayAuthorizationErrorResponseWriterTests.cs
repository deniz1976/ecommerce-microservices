using System.Text.Json;
using ECommerce.ApiGateway.Middleware;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ContractTests;

public sealed class GatewayAuthorizationErrorResponseWriterTests
{
    [Theory]
    [InlineData(401, "en", ErrorCodes.AuthenticationRequired, "Authentication is required.")]
    [InlineData(401, "tr", ErrorCodes.AuthenticationRequired, "Kimlik doğrulaması gereklidir.")]
    [InlineData(403, "en", ErrorCodes.AccessDenied, "Access to the requested resource is denied.")]
    [InlineData(403, "tr", ErrorCodes.AccessDenied, "İstenen kaynağa erişim reddedildi.")]
    public async Task EmptyAuthorizationFailure_ReturnsLocalizedApiError(
        int statusCode,
        string culture,
        string expectedCode,
        string expectedMessage)
    {
        await using ServiceProvider services = new ServiceCollection()
            .AddECommerceLocalization()
            .BuildServiceProvider();
        DefaultHttpContext context = CreateContext(services, statusCode, culture);

        await GatewayAuthorizationErrorResponseWriter.WriteIfNeededAsync(context);

        context.Response.Body.Position = 0;
        using JsonDocument response = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(statusCode, context.Response.StatusCode);
        Assert.Equal(expectedCode, response.RootElement.GetProperty("code").GetString());
        Assert.Equal(expectedMessage, response.RootElement.GetProperty("message").GetString());
        Assert.Equal(
            statusCode == StatusCodes.Status401Unauthorized ? "Bearer" : string.Empty,
            context.Response.Headers.WWWAuthenticate.ToString());
    }

    [Fact]
    public async Task ExistingAuthorizationErrorResponse_IsPreserved()
    {
        await using ServiceProvider services = new ServiceCollection()
            .AddECommerceLocalization()
            .BuildServiceProvider();
        DefaultHttpContext context = CreateContext(
            services,
            StatusCodes.Status403Forbidden,
            "tr");
        context.Response.ContentType = "application/problem+json";
        byte[] existingBody = """{"existing":true}"""u8.ToArray();
        await context.Response.Body.WriteAsync(existingBody);

        await GatewayAuthorizationErrorResponseWriter.WriteIfNeededAsync(context);

        Assert.Equal(existingBody, ((MemoryStream)context.Response.Body).ToArray());
        Assert.Equal("application/problem+json", context.Response.ContentType);
    }

    private static DefaultHttpContext CreateContext(
        IServiceProvider services,
        int statusCode,
        string culture)
    {
        DefaultHttpContext context = new()
        {
            RequestServices = services,
            Response =
            {
                Body = new MemoryStream(),
                StatusCode = statusCode
            }
        };
        context.Request.Headers.AcceptLanguage = culture;
        return context;
    }
}
