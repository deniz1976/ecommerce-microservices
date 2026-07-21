namespace ECommerce.ContractTests;

internal sealed record RecordedRequest(
    HttpMethod Method,
    string Path,
    string? AuthorizationToken,
    string Body);
