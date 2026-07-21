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
}
