using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ECommerce.BuildingBlocks.Persistence;

public sealed class UtcTimestampSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        NormalizeTrackedTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        NormalizeTrackedTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    internal static void NormalizeTrackedTimestamps(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        IEnumerable<EntityEntry> entries = dbContext.ChangeTracker
            .Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified);

        IEnumerable<PropertyEntry> timestampProperties = entries
            .SelectMany(entry => entry.Properties)
            .Where(property =>
                property.Metadata.FindAnnotation(UtcTimestampPropertyExtensions.AnnotationName)?.Value is true);

        foreach (PropertyEntry property in timestampProperties)
        {
            property.CurrentValue = property.CurrentValue switch
            {
                DateTimeOffset value => UtcTimestampNormalizer.Normalize(value),
                DateTime value => UtcTimestampNormalizer.Normalize(value),
                _ => property.CurrentValue
            };
        }
    }
}
