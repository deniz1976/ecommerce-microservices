namespace ECommerce.Catalog.Domain;

public sealed class Store
{
    private Store()
    {
        Name = string.Empty;
        Slug = string.Empty;
    }

    public Store(Guid id, Guid ownerUserId, string name, string slug)
    {
        Id = id;
        OwnerUserId = ownerUserId;
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public string Name { get; private set; }

    public string Slug { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string name, string slug, DateTimeOffset updatedAt)
    {
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        UpdatedAt = updatedAt.ToUniversalTime();
    }
}
