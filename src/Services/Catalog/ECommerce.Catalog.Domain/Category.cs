namespace ECommerce.Catalog.Domain;

public sealed class Category
{
    private readonly List<CategoryTranslation> translations = [];

    private Category()
    {
        Slug = string.Empty;
    }

    public Category(Guid id, string slug, bool isActive)
    {
        Id = id;
        Slug = slug;
        IsActive = isActive;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Slug { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<CategoryTranslation> Translations => translations;

    public void Update(string slug, bool isActive)
    {
        Slug = slug;
        IsActive = isActive;
    }

    public void SetTranslation(string languageCode, string name)
    {
        CategoryTranslation? translation = translations.FirstOrDefault(x => x.LanguageCode == languageCode);

        if (translation is null)
        {
            translations.Add(new CategoryTranslation(Id, languageCode, name));
            return;
        }

        translation.Update(name);
    }
}
