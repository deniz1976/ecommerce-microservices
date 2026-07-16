namespace ECommerce.Catalog.Domain;

public sealed class CategoryTranslation
{
    private CategoryTranslation()
    {
        LanguageCode = string.Empty;
        Name = string.Empty;
    }

    public CategoryTranslation(Guid categoryId, string languageCode, string name)
    {
        CategoryId = categoryId;
        LanguageCode = languageCode;
        Name = name;
    }

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public string LanguageCode { get; private set; }

    public string Name { get; private set; }

    public void Update(string name)
    {
        Name = name;
    }
}
