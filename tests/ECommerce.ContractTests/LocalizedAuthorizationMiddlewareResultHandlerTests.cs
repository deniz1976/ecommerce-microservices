using System.Text.Json;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Security;
using ECommerce.ContractTests.Support;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ContractTests;

public sealed class LocalizedAuthorizationMiddlewareResultHandlerTests
{
    [Theory]
    [InlineData(true, "en", 401, ErrorCodes.AuthenticationRequired, "Authentication is required.")]
    [InlineData(true, "tr", 401, ErrorCodes.AuthenticationRequired, "Kimlik doğrulaması gereklidir.")]
    [InlineData(false, "en", 403, ErrorCodes.AccessDenied, "Access to the requested resource is denied.")]
    [InlineData(false, "tr", 403, ErrorCodes.AccessDenied, "İstenen kaynağa erişim reddedildi.")]
    public async Task AuthorizationFailure_ReturnsLocalizedApiError(
        bool challenged,
        string culture,
        int expectedStatus,
        string expectedCode,
        string expectedMessage)
    {
        ServiceProvider services = new ServiceCollection()
            .AddSingleton<IAuthenticationService, RecordingAuthenticationService>()
            .AddECommerceLocalization()
            .BuildServiceProvider();
        DefaultHttpContext context = new()
        {
            RequestServices = services,
            Response = { Body = new MemoryStream() }
        };
        context.Request.Headers.AcceptLanguage = culture;
        LocalizedAuthorizationMiddlewareResultHandler handler = new();
        PolicyAuthorizationResult result = challenged
            ? PolicyAuthorizationResult.Challenge()
            : PolicyAuthorizationResult.Forbid();

        await handler.HandleAsync(
            _ => Task.CompletedTask,
            context,
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(),
            result);

        context.Response.Body.Position = 0;
        using JsonDocument response = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.Equal(expectedCode, response.RootElement.GetProperty("code").GetString());
        Assert.Equal(expectedMessage, response.RootElement.GetProperty("message").GetString());
        Assert.False(string.IsNullOrWhiteSpace(response.RootElement.GetProperty("traceId").GetString()));
        Assert.Equal(challenged ? "Bearer" : string.Empty, context.Response.Headers.WWWAuthenticate.ToString());
    }

    [Fact]
    public void AddOidcReadySecurity_RegistersLocalizedAuthorizationHandler()
    {
        ServiceProvider services = new ServiceCollection()
            .AddECommerceLocalization()
            .AddOidcReadySecurity(new ConfigurationBuilder().Build())
            .BuildServiceProvider();

        IAuthorizationMiddlewareResultHandler handler =
            services.GetRequiredService<IAuthorizationMiddlewareResultHandler>();

        Assert.IsType<LocalizedAuthorizationMiddlewareResultHandler>(handler);
    }
}
