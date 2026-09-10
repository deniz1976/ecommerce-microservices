using System.Buffers.Binary;

namespace ECommerce.Catalog.Application.Products;

internal static class ProductImageDimensionReader
{
    private const int MaximumHeaderBytes = 64 * 1024;

    internal static async Task<(int Width, int Height)?> ReadAsync(
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        byte[] bytes = new byte[Math.Min(MaximumHeaderBytes, checked((int)Math.Min(content.Length, MaximumHeaderBytes)))];
        int totalRead = 0;
        while (totalRead < bytes.Length)
        {
            int read = await content.ReadAsync(bytes.AsMemory(totalRead), cancellationToken);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        content.Position = 0;
        ReadOnlySpan<byte> header = bytes.AsSpan(0, totalRead);
        return contentType.ToLowerInvariant() switch
        {
            "image/png" => ReadPng(header),
            "image/gif" => ReadGif(header),
            "image/jpeg" => ReadJpeg(header),
            "image/webp" => ReadWebP(header),
            _ => null
        };
    }

    private static (int Width, int Height)? ReadPng(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= 24 && bytes[12..16].SequenceEqual("IHDR"u8)
            ? (ReadBigEndianInt(bytes[16..20]), ReadBigEndianInt(bytes[20..24]))
            : null;

    private static (int Width, int Height)? ReadGif(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= 10
            ? (BinaryPrimitives.ReadUInt16LittleEndian(bytes[6..8]), BinaryPrimitives.ReadUInt16LittleEndian(bytes[8..10]))
            : null;

    private static (int Width, int Height)? ReadJpeg(ReadOnlySpan<byte> bytes)
    {
        int offset = 2;
        while (offset + 8 < bytes.Length)
        {
            if (bytes[offset] != 0xFF)
            {
                offset++;
                continue;
            }

            byte marker = bytes[offset + 1];
            if (marker is 0xD8 or 0xD9 || marker is >= 0xD0 and <= 0xD7)
            {
                offset += 2;
                continue;
            }

            int segmentLength = BinaryPrimitives.ReadUInt16BigEndian(bytes[(offset + 2)..(offset + 4)]);
            if (segmentLength < 2 || offset + 2 + segmentLength > bytes.Length)
            {
                return null;
            }

            if (marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF)
            {
                return (
                    BinaryPrimitives.ReadUInt16BigEndian(bytes[(offset + 7)..(offset + 9)]),
                    BinaryPrimitives.ReadUInt16BigEndian(bytes[(offset + 5)..(offset + 7)]));
            }

            offset += 2 + segmentLength;
        }

        return null;
    }

    private static (int Width, int Height)? ReadWebP(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 30)
        {
            return null;
        }

        ReadOnlySpan<byte> format = bytes[12..16];
        if (format.SequenceEqual("VP8X"u8))
        {
            return (ReadUInt24(bytes[24..27]) + 1, ReadUInt24(bytes[27..30]) + 1);
        }

        if (format.SequenceEqual("VP8L"u8) && bytes[20] == 0x2F)
        {
            int bits = BinaryPrimitives.ReadInt32LittleEndian(bytes[21..25]);
            return ((bits & 0x3FFF) + 1, ((bits >> 14) & 0x3FFF) + 1);
        }

        if (format.SequenceEqual("VP8 "u8) && bytes[23..26].SequenceEqual(new byte[] { 0x9D, 0x01, 0x2A }))
        {
            return (
                BinaryPrimitives.ReadUInt16LittleEndian(bytes[26..28]) & 0x3FFF,
                BinaryPrimitives.ReadUInt16LittleEndian(bytes[28..30]) & 0x3FFF);
        }

        return null;
    }

    private static int ReadBigEndianInt(ReadOnlySpan<byte> bytes) =>
        BinaryPrimitives.ReadInt32BigEndian(bytes);

    private static int ReadUInt24(ReadOnlySpan<byte> bytes) =>
        bytes[0] | (bytes[1] << 8) | (bytes[2] << 16);
}
