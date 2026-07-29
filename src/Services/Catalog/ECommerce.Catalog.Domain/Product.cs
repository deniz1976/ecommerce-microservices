namespace ECommerce.Catalog.Domain;

public sealed class Product
{
    private readonly List<ProductTranslation> translations = [];
    private readonly List<ProductImage> images = [];

    private Product()
    {
        Sku = string.Empty;
        Currency = string.Empty;
    }

    public Product(
        Guid id,
        string sku,
        Guid categoryId,
        Guid brandId,
        Guid? storeId,
        decimal price,
        string currency,
        ProductStatus status)
    {
        Id = id;
        Sku = sku;
        CategoryId = categoryId;
        BrandId = brandId;
        StoreId = storeId;
        Price = price;
        Currency = currency;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public string Sku { get; private set; }

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public Guid BrandId { get; private set; }

    public Brand? Brand { get; private set; }

    public Guid? StoreId { get; private set; }

    public Store? Store { get; private set; }

    public decimal Price { get; private set; }

    public string Currency { get; private set; }

    public ProductStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<ProductTranslation> Translations => translations;

    public IReadOnlyCollection<ProductImage> Images => images;

    public void UpdateDetails(Guid categoryId, Guid brandId, decimal price, string currency, ProductStatus status)
    {
        CategoryId = categoryId;
        BrandId = brandId;
        Price = price;
        Currency = currency;
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetTranslation(string languageCode, string name, string description)
    {
        ProductTranslation? translation = translations.FirstOrDefault(x => x.LanguageCode == languageCode);

        if (translation is null)
        {
            translations.Add(new ProductTranslation(Id, languageCode, name, description));
            UpdatedAt = DateTimeOffset.UtcNow;
            return;
        }

        translation.Update(name, description);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddImage(ProductImage image)
    {
        if (image.IsMain)
        {
            foreach (ProductImage existingImage in images)
            {
                existingImage.MarkAsSecondary();
            }
        }

        images.Add(image);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetMainImage(Guid imageId)
    {
        ProductImage? selectedImage = images.FirstOrDefault(image => image.Id == imageId);
        if (selectedImage is null)
        {
            return;
        }

        foreach (ProductImage image in images)
        {
            if (image.Id == imageId)
            {
                image.MarkAsMain();
            }
            else
            {
                image.MarkAsSecondary();
            }
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public ProductImage? RemoveImage(Guid imageId)
    {
        ProductImage? image = images.FirstOrDefault(item => item.Id == imageId);
        if (image is null)
        {
            return null;
        }

        images.Remove(image);
        if (image.IsMain && images.Count > 0)
        {
            images.OrderBy(item => item.SortOrder).First().MarkAsMain();
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        return image;
    }
}
