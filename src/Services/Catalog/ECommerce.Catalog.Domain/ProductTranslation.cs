namespace ECommerce.Catalog.Domain;

public sealed class ProductTranslation
{
    private ProductTranslation()
    {
        LanguageCode = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
    }

    public ProductTranslation(Guid productId, string languageCode, string name, string description)
    {
        ProductId = productId;
        LanguageCode = languageCode;
        Name = name;
        Description = description;
    }

    public Guid ProductId { get; private set; }

    public Product? Product { get; private set; }

    public string LanguageCode { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
