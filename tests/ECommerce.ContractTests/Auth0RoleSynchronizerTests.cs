using System.Net;
using System.Text;
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
        using Auth0RoleSynchronizer synchronizer = CreateSynchronizer(handler);

        bool result = await synchronizer.SynchronizeSelfServiceRoleAsync("auth0|user-1", "Customer", default);

        Assert.True(result);
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
        using Auth0RoleSynchronizer synchronizer = CreateSynchronizer(handler);

        bool result = await synchronizer.SynchronizeSelfServiceRoleAsync("github|user-2", "Seller", default);

        Assert.False(result);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task DisabledSynchronizationDoesNotCallAuth0()
    {
        RecordingHandler handler = new(_ => throw new InvalidOperationException("Auth0 should not be called."));
        using Auth0RoleSynchronizer synchronizer = CreateSynchronizer(handler, enabled: false);

        bool result = await synchronizer.SynchronizeSelfServiceRoleAsync("auth0|user-3", "Customer", default);

        Assert.True(result);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task MalformedTokenResponseFailsClosed()
    {
        RecordingHandler handler = new(_ => Json(HttpStatusCode.OK, "{}"));
        using Auth0RoleSynchronizer synchronizer = CreateSynchronizer(handler);

        bool result = await synchronizer.SynchronizeSelfServiceRoleAsync("auth0|user-4", "Customer", default);

        Assert.False(result);
        Assert.Single(handler.Requests);
    }

    private static Auth0RoleSynchronizer CreateSynchronizer(RecordingHandler handler, bool enabled = true)
    {
        HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://tenant.auth0.com/")
        };

        return new Auth0RoleSynchronizer(
            new SingleHttpClientFactory(client),
            Options.Create(new Auth0ManagementOptions
            {
                Enabled = enabled,
                Domain = "tenant.auth0.com",
                ClientId = "client-id",
                ClientSecret = "client-secret",
                CustomerRoleId = "customer-role",
                SellerRoleId = "seller-role"
            }),
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

    private sealed class SingleHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(new RecordedRequest(
                request.Method,
                request.RequestUri?.AbsolutePath ?? string.Empty,
                request.Headers.Authorization?.Parameter,
                request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken)));

            return responseFactory(request);
        }
    }

    private sealed record RecordedRequest(
        HttpMethod Method,
        string Path,
        string? AuthorizationToken,
        string Body);
}
