using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using ECommerce.BuildingBlocks.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ECommerce.ContractTests;

public sealed class CustomerOwnershipAuthorizerTests
{
    [Theory]
    [InlineData("not-a-url", "5")]
    [InlineData("http://identity.test", "0")]
    [InlineData("http://identity.test", "31")]
    public void RegistrationRejectsUnsafeConfiguration(string baseUrl, string timeoutSeconds)
    {
        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{CustomerIdentityOptions.SectionName}:BaseUrl"] = baseUrl,
                [$"{CustomerIdentityOptions.SectionName}:TimeoutSeconds"] = timeoutSeconds
            })
            .Build();

        Assert.Throws<InvalidOperationException>(() => services.AddCustomerOwnership(configuration));
    }

    [Fact]
    public async Task OwnerCanAccessMatchingCustomerId()
    {
        Guid customerId = Guid.NewGuid();
        StubHttpMessageHandler handler = new(_ => JsonResponse(customerId));
        ICustomerOwnershipAuthorizer authorizer = CreateAuthorizer(
            AuthenticatedPrincipal(),
            handler,
            "owner-token");

        bool allowed = await authorizer.CanAccessAsync(customerId, CancellationToken.None);

        Assert.True(allowed);
        Assert.Equal(1, handler.RequestCount);
        Assert.Equal("Bearer", handler.LastRequest?.Headers.Authorization?.Scheme);
        Assert.Equal("owner-token", handler.LastRequest?.Headers.Authorization?.Parameter);
        Assert.Equal("http://identity.test/api/v1/auth/me", handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public async Task OwnerCannotAccessAnotherCustomerId()
    {
        StubHttpMessageHandler handler = new(_ => JsonResponse(Guid.NewGuid()));
        ICustomerOwnershipAuthorizer authorizer = CreateAuthorizer(
            AuthenticatedPrincipal(),
            handler,
            "owner-token");

        bool allowed = await authorizer.CanAccessAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(allowed);
    }

    [Fact]
    public async Task NotificationHubQueryTokenIsForwardedToIdentity()
    {
        Guid customerId = Guid.NewGuid();
        StubHttpMessageHandler handler = new(_ => JsonResponse(customerId));
        ICustomerOwnershipAuthorizer authorizer = CreateAuthorizer(
            AuthenticatedPrincipal(),
            handler,
            accessToken: null,
            path: "/hubs/notifications",
            queryToken: "hub-token");

        bool allowed = await authorizer.CanAccessAsync(customerId, CancellationToken.None);

        Assert.True(allowed);
        Assert.Equal("hub-token", handler.LastRequest?.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task QueryTokenOutsideNotificationHubIsRejected()
    {
        StubHttpMessageHandler handler = new(_ => throw new InvalidOperationException("Identity should not be called."));
        ICustomerOwnershipAuthorizer authorizer = CreateAuthorizer(
            AuthenticatedPrincipal(),
            handler,
            accessToken: null,
            path: "/api/v1/baskets/customer",
            queryToken: "query-token");

        bool allowed = await authorizer.CanAccessAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(allowed);
        Assert.Equal(0, handler.RequestCount);
    }

    [Theory]
    [InlineData(AuthOptions.DefaultRoleClaimType, ApplicationRoles.Admin)]
    [InlineData("permissions", ApplicationPermissions.ActAsCustomer)]
    [InlineData("scope", "openid customer:act")]
    public async Task PrivilegedCallerCanActForCustomerWithoutIdentityLookup(string claimType, string claimValue)
    {
        ClaimsPrincipal principal = AuthenticatedPrincipal(new Claim(claimType, claimValue));
        StubHttpMessageHandler handler = new(_ => throw new InvalidOperationException("Identity should not be called."));
        ICustomerOwnershipAuthorizer authorizer = CreateAuthorizer(principal, handler, "automation-token");

        bool allowed = await authorizer.CanAccessAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(allowed);
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task IdentityFailureDeniesAccess()
    {
        StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        ICustomerOwnershipAuthorizer authorizer = CreateAuthorizer(
            AuthenticatedPrincipal(),
            handler,
            "owner-token");

        bool allowed = await authorizer.CanAccessAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(allowed);
    }

    [Fact]
    public async Task InvalidIdentityResponseDeniesAccess()
    {
        StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json")
        });
        ICustomerOwnershipAuthorizer authorizer = CreateAuthorizer(
            AuthenticatedPrincipal(),
            handler,
            "owner-token");

        bool allowed = await authorizer.CanAccessAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(allowed);
    }

    [Fact]
    public async Task IdentityTimeoutDeniesAccess()
    {
        StubHttpMessageHandler handler = new(_ => throw new TaskCanceledException("Timed out."));
        ICustomerOwnershipAuthorizer authorizer = CreateAuthorizer(
            AuthenticatedPrincipal(),
            handler,
            "owner-token");

        bool allowed = await authorizer.CanAccessAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(allowed);
    }

    private static ICustomerOwnershipAuthorizer CreateAuthorizer(
        ClaimsPrincipal principal,
        StubHttpMessageHandler handler,
        string? accessToken,
        string path = "/",
        string? queryToken = null)
    {
        DefaultHttpContext httpContext = new();
        httpContext.User = principal;
        httpContext.Request.Path = path;
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            httpContext.Request.Headers.Authorization = $"Bearer {accessToken}";
        }

        if (!string.IsNullOrWhiteSpace(queryToken))
        {
            httpContext.Request.QueryString = new QueryString($"?access_token={queryToken}");
        }

        HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("http://identity.test/")
        };

        HttpContextAccessor accessor = new() { HttpContext = httpContext };
        IdentityAuthenticatedUserResolver resolver = new(
            httpClient,
            accessor,
            NullLogger<IdentityAuthenticatedUserResolver>.Instance);
        return new IdentityCustomerOwnershipAuthorizer(resolver, accessor);
    }

    private static ClaimsPrincipal AuthenticatedPrincipal(params Claim[] claims)
    {
        ClaimsIdentity identity = new(
            claims,
            "Test",
            ClaimTypes.Name,
            AuthOptions.DefaultRoleClaimType);
        return new ClaimsPrincipal(identity);
    }

    private static HttpResponseMessage JsonResponse(Guid customerId)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { id = customerId })
        };
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> responseFactory;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            this.responseFactory = responseFactory;
        }

        public int RequestCount { get; private set; }
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            LastRequest = request;
            return Task.FromResult(responseFactory(request));
        }
    }
}
