using System.Net;
using System.Text;
using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Infrastructure.Auth0;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ECommerce.ContractTests;

public sealed class Auth0RoleSynchronizerTests
{
    [Fact]
    public async Task CustomerSelectionRemovesSellerThenAssignsCustomer()
    {
        RecordingHandler handler = new(request => request.RequestUri?.AbsolutePath switch
        {
            "/oauth/token" => Json(HttpStatusCode.OK, "{\"access_token\":\"management-token\",\"expires_in\":3600}"),
            "/api/v2/users/auth0%7Cuser-1/roles" when request.Method == HttpMethod.Delete =>
                new HttpResponseMessage(HttpStatusCode.NoContent),
            "/api/v2/users/auth0%7Cuser-1/roles" when request.Method == HttpMethod.Post =>
                new HttpResponseMessage(HttpStatusCode.NoContent),
            _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        });
        Auth0RoleSynchronizer synchronizer = CreateSynchronizer(handler);

        ExternalRoleSynchronizationResult result = await synchronizer.SynchronizeSelfServiceRoleAsync(
            "auth0|user-1",
            "Customer",
            "Seller",
            default);

        Assert.Equal(ExternalRoleSynchronizationResult.Succeeded, result);
        Assert.Collection(
            handler.Requests,
            request => Assert.Equal((HttpMethod.Post, "/oauth/token", null), RequestSummary(request)),
            request =>
            {
                Assert.Equal((HttpMethod.Delete, "/api/v2/users/auth0%7Cuser-1/roles", "management-token"), RequestSummary(request));
                Assert.Contains("seller-role", request.Body, StringComparison.Ordinal);
            },
            request =>
            {
                Assert.Equal((HttpMethod.Post, "/api/v2/users/auth0%7Cuser-1/roles", "management-token"), RequestSummary(request));
                Assert.Contains("customer-role", request.Body, StringComparison.Ordinal);
            });
    }

    [Fact]
    public async Task ManagementApiFailureFailsClosedBeforeRoleAssignment()
    {
        RecordingHandler handler = new(request => request.RequestUri?.AbsolutePath switch
        {
            "/oauth/token" => Json(HttpStatusCode.OK, "{\"access_token\":\"management-token\",\"expires_in\":3600}"),
            _ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        });
        Auth0RoleSynchronizer synchronizer = CreateSynchronizer(handler);

        ExternalRoleSynchronizationResult result = await synchronizer.SynchronizeSelfServiceRoleAsync(
            "github|user-2",
            "Seller",
            "Customer",
            default);

        Assert.Equal(ExternalRoleSynchronizationResult.FailedRestored, result);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task AssignmentFailureRestoresPreviousRole()
    {
        RecordingHandler handler = new(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/oauth/token")
            {
                return Json(HttpStatusCode.OK, "{\"access_token\":\"management-token\",\"expires_in\":3600}");
            }

            if (request.Method == HttpMethod.Delete)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return request.Content?.ReadAsStringAsync().GetAwaiter().GetResult() switch
            {
                string body when body.Contains("seller-role", StringComparison.Ordinal) =>
                    new HttpResponseMessage(HttpStatusCode.BadGateway),
                string body when body.Contains("customer-role", StringComparison.Ordinal) =>
                    new HttpResponseMessage(HttpStatusCode.NoContent),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            };
        });
        Auth0RoleSynchronizer synchronizer = CreateSynchronizer(handler);

        ExternalRoleSynchronizationResult result = await synchronizer.SynchronizeSelfServiceRoleAsync(
            "auth0|user-5",
            "Seller",
            "Customer",
            default);

        Assert.Equal(ExternalRoleSynchronizationResult.FailedRestored, result);
        Assert.Collection(
            handler.Requests,
            request => Assert.Equal(HttpMethod.Post, request.Method),
            request => Assert.Equal(HttpMethod.Delete, request.Method),
            request => Assert.Contains("seller-role", request.Body, StringComparison.Ordinal),
            request => Assert.Contains("customer-role", request.Body, StringComparison.Ordinal));
    }

    [Fact]
    public async Task AssignmentAndRestorationFailureRequiresReconciliation()
    {
        RecordingHandler handler = new(request => request.RequestUri?.AbsolutePath switch
        {
            "/oauth/token" => Json(
                HttpStatusCode.OK,
                "{\"access_token\":\"management-token\",\"expires_in\":3600}"),
            _ when request.Method == HttpMethod.Delete =>
                new HttpResponseMessage(HttpStatusCode.NoContent),
            _ => new HttpResponseMessage(HttpStatusCode.BadGateway)
        });
        Auth0RoleSynchronizer synchronizer = CreateSynchronizer(handler);

        ExternalRoleSynchronizationResult result =
            await synchronizer.SynchronizeSelfServiceRoleAsync(
                "auth0|user-6",
                "Seller",
                "Customer",
                default);

        Assert.Equal(ExternalRoleSynchronizationResult.ReconciliationRequired, result);
        Assert.Equal(4, handler.Requests.Count);
    }

    [Fact]
    public async Task DisabledSynchronizationDoesNotCallAuth0()
    {
        RecordingHandler handler = new(_ => throw new InvalidOperationException("Auth0 should not be called."));
        Auth0RoleSynchronizer synchronizer = CreateSynchronizer(handler, enabled: false);

        ExternalRoleSynchronizationResult result = await synchronizer.SynchronizeSelfServiceRoleAsync(
            "auth0|user-3",
            "Customer",
            "Seller",
            default);

        Assert.Equal(ExternalRoleSynchronizationResult.Succeeded, result);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task MalformedTokenResponseFailsClosed()
    {
        RecordingHandler handler = new(_ => Json(HttpStatusCode.OK, "{}"));
        Auth0RoleSynchronizer synchronizer = CreateSynchronizer(handler);

        ExternalRoleSynchronizationResult result = await synchronizer.SynchronizeSelfServiceRoleAsync(
            "auth0|user-4",
            "Customer",
            "Seller",
            default);

        Assert.Equal(ExternalRoleSynchronizationResult.FailedRestored, result);
        Assert.Single(handler.Requests);
    }

    private static Auth0RoleSynchronizer CreateSynchronizer(RecordingHandler handler, bool enabled = true)
    {
        HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://tenant.auth0.com/")
        };

        SingleHttpClientFactory httpClientFactory = new(client);
        IOptions<Auth0ManagementOptions> options = Options.Create(new Auth0ManagementOptions
        {
            Enabled = enabled,
            Domain = "tenant.auth0.com",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            CustomerRoleId = "customer-role",
            SellerRoleId = "seller-role"
        });
        Auth0ManagementTokenProvider tokenProvider = new(httpClientFactory, options);

        return new Auth0RoleSynchronizer(
            httpClientFactory,
            tokenProvider,
            options,
            NullLogger<Auth0RoleSynchronizer>.Instance);
    }

    private static HttpResponseMessage Json(HttpStatusCode statusCode, string body)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }

    private static (HttpMethod Method, string Path, string? Token) RequestSummary(RecordedRequest request)
    {
        return (request.Method, request.Path, request.AuthorizationToken);
    }

}
