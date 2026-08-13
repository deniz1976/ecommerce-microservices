namespace ECommerce.Ordering.Domain;

public enum OrderItemMutationResult
{
    Applied = 0,
    InvalidItem = 1,
    CurrencyMismatch = 2,
    DuplicateProduct = 3
}
