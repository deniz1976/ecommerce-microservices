namespace ECommerce.BuildingBlocks.Contracts.Errors;

public sealed record Error(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Details = null);
