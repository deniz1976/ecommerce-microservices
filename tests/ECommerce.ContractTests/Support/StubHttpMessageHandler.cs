namespace ECommerce.ContractTests;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
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
