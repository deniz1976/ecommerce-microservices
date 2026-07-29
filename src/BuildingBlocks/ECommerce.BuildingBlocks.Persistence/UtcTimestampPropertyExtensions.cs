using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.BuildingBlocks.Persistence;

public static class UtcTimestampPropertyExtensions
{
    internal const string AnnotationName = "ECommerce:IsUtcTimestamp";
    private const string PostgreSqlUtcTimestampType = "timestamp with time zone";

    public static PropertyBuilder<DateTimeOffset> IsUtcTimestamp(
        this PropertyBuilder<DateTimeOffset> propertyBuilder)
    {
        return propertyBuilder
            .HasColumnType(PostgreSqlUtcTimestampType)
            .HasAnnotation(AnnotationName, true);
    }

    public static PropertyBuilder<DateTimeOffset?> IsUtcTimestamp(
        this PropertyBuilder<DateTimeOffset?> propertyBuilder)
    {
        return propertyBuilder
            .HasColumnType(PostgreSqlUtcTimestampType)
            .HasAnnotation(AnnotationName, true);
    }
}
