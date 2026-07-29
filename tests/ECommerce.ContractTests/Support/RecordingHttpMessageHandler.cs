namespace ECommerce.ContractTests;

internal sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> responseFactory;

    public RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        this.responseFactory = responseFactory;
    }

    public HttpMethod? Method { get; private set; }

    public Uri? RequestUri { get; private set; }

    public string? AuthenticationScheme { get; private set; }

    public string? AuthenticationParameter { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Method = request.Method;
        RequestUri = request.RequestUri;
        AuthenticationScheme = request.Headers.Authorization?.Scheme;
        AuthenticationParameter = request.Headers.Authorization?.Parameter;
        return Task.FromResult(responseFactory(request));
    }
}
