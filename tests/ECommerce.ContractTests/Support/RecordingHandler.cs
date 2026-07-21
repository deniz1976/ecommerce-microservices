namespace ECommerce.ContractTests;

internal sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
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
