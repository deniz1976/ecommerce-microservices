namespace ECommerce.BuildingBlocks.Contracts.Api;

public sealed record ApiErrorResponse(
    string TraceId,
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Details = null);
