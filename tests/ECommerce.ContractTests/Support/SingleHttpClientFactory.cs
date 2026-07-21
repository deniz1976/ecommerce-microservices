namespace ECommerce.ContractTests;

internal sealed class SingleHttpClientFactory(HttpClient client) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => client;
}
