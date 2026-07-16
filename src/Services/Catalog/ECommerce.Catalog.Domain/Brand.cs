namespace ECommerce.Catalog.Domain;

public sealed class Brand
{
    private Brand()
    {
        Name = string.Empty;
        Slug = string.Empty;
    }

    public Brand(Guid id, string name, string slug, bool isActive)
    {
        Id = id;
        Name = name;
        Slug = slug;
        IsActive = isActive;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Slug { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
