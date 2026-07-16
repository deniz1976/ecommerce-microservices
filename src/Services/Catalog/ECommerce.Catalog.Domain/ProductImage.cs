namespace ECommerce.Catalog.Domain;

public sealed class ProductImage
{
    private ProductImage()
    {
        PublicId = string.Empty;
        Url = string.Empty;
        SecureUrl = string.Empty;
        Format = string.Empty;
    }

    public ProductImage(
        Guid id,
        Guid productId,
        string publicId,
        string url,
        string secureUrl,
        int width,
        int height,
        string format,
        int sortOrder,
        bool isMain)
    {
        Id = id;
        ProductId = productId;
        PublicId = publicId;
        Url = url;
        SecureUrl = secureUrl;
        Width = width;
        Height = height;
        Format = format;
        SortOrder = sortOrder;
        IsMain = isMain;
    }

    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public Product? Product { get; private set; }

    public string PublicId { get; private set; }

    public string Url { get; private set; }

    public string SecureUrl { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public string Format { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsMain { get; private set; }

    public void MarkAsSecondary()
    {
        IsMain = false;
    }
}
