namespace ECommerce.Catalog.Application;

public static class CatalogErrorCodes
{
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string BrandNotFound = "BRAND_NOT_FOUND";
    public const string InvalidProductTranslation = "INVALID_PRODUCT_TRANSLATION";
    public const string StoreNotFound = "STORE_NOT_FOUND";
    public const string StoreRequired = "STORE_REQUIRED";
    public const string StoreAccessDenied = "STORE_ACCESS_DENIED";
    public const string StoreSlugConflict = "STORE_SLUG_CONFLICT";
    public const string IdentityResolutionFailed = "IDENTITY_RESOLUTION_FAILED";
    public const string InvalidProductImage = "INVALID_PRODUCT_IMAGE";
    public const string ProductImageLimitExceeded = "PRODUCT_IMAGE_LIMIT_EXCEEDED";
    public const string ProductImageNotFound = "PRODUCT_IMAGE_NOT_FOUND";
    public const string ImageStorageUnavailable = "IMAGE_STORAGE_UNAVAILABLE";
}
