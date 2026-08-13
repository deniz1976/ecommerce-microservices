using System.Text.RegularExpressions;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Catalog.Application.References;

internal static partial class CatalogReferenceInput
{
    public static string NormalizeSlug(string value) =>
        value.Trim().ToLowerInvariant();

    public static bool IsValidSlug(string value) =>
        value.Length is >= 2 and <= 160 && SlugRegex().IsMatch(value);

    public static bool IsValidName(string value) =>
        value.Length is >= 2 and <= 256;

    public static Result<T> ValidationFailure<T>() =>
        Result<T>.Failure(new Error(
            ErrorCodes.ValidationFailed,
            ErrorCodes.ValidationFailed));

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugRegex();
}
