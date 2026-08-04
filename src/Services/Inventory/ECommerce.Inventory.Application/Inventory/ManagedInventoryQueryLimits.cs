namespace ECommerce.Inventory.Application.Inventory;

public static class ManagedInventoryQueryLimits
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int MaxQuantityFilter = 1_000_000_000;
}
