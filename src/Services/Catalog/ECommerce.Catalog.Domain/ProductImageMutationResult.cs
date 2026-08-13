namespace ECommerce.Catalog.Domain;

public enum ProductImageMutationResult
{
    Applied = 0,
    LimitExceeded = 1,
    InvalidImage = 2,
    DuplicateImage = 3
}
