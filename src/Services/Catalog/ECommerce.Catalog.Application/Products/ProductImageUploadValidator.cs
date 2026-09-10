using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Images;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductImageUploadValidator
{
    public const long MaxFileSize = 5 * 1024 * 1024;
    public const int MaxDimension = 8_192;
    public const long MaxPixelCount = 40_000_000;

    public async Task<Result> ValidateAsync(
        ProductImageUpload upload,
        CancellationToken cancellationToken)
    {
        if (upload.Length is <= 0 or > MaxFileSize ||
            !upload.Content.CanRead ||
            !upload.Content.CanSeek)
        {
            return Invalid();
        }

        byte[] header = new byte[12];
        int bytesRead = await upload.Content.ReadAsync(header, cancellationToken);
        upload.Content.Position = 0;

        bool signatureMatches = upload.ContentType.ToLowerInvariant() switch
        {
            "image/jpeg" => IsJpeg(header, bytesRead),
            "image/png" => IsPng(header, bytesRead),
            "image/gif" => IsGif(header, bytesRead),
            "image/webp" => IsWebP(header, bytesRead),
            _ => false
        };

        if (!signatureMatches)
        {
            return Invalid();
        }

        (int Width, int Height)? dimensions = await ProductImageDimensionReader.ReadAsync(
            upload.Content,
            upload.ContentType,
            cancellationToken);

        return dimensions is { Width: > 0 and <= MaxDimension, Height: > 0 and <= MaxDimension } value &&
               (long)value.Width * value.Height <= MaxPixelCount
            ? Result.Success()
            : Invalid();
    }

    private static bool IsJpeg(IReadOnlyList<byte> bytes, int length) =>
        length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;

    private static bool IsPng(IReadOnlyList<byte> bytes, int length) =>
        length >= 8 &&
        bytes[0] == 0x89 &&
        bytes[1] == 0x50 &&
        bytes[2] == 0x4E &&
        bytes[3] == 0x47 &&
        bytes[4] == 0x0D &&
        bytes[5] == 0x0A &&
        bytes[6] == 0x1A &&
        bytes[7] == 0x0A;

    private static bool IsGif(IReadOnlyList<byte> bytes, int length) =>
        length >= 6 &&
        bytes[0] == 0x47 &&
        bytes[1] == 0x49 &&
        bytes[2] == 0x46 &&
        bytes[3] == 0x38 &&
        (bytes[4] == 0x37 || bytes[4] == 0x39) &&
        bytes[5] == 0x61;

    private static bool IsWebP(IReadOnlyList<byte> bytes, int length) =>
        length >= 12 &&
        bytes[0] == 0x52 &&
        bytes[1] == 0x49 &&
        bytes[2] == 0x46 &&
        bytes[3] == 0x46 &&
        bytes[8] == 0x57 &&
        bytes[9] == 0x45 &&
        bytes[10] == 0x42 &&
        bytes[11] == 0x50;

    private static Result Invalid() =>
        Result.Failure(new Error(CatalogErrorCodes.InvalidProductImage, CatalogErrorCodes.InvalidProductImage));
}
