namespace ECommerce.BuildingBlocks.Security;

public static class AuthorizationPolicies
{
    public const string AuthenticatedUser = "AuthenticatedUser";

    public const string Admin = "Admin";

    public const string InventoryWrite = "InventoryWrite";

    public const string SellerOrAdmin = "SellerOrAdmin";

    public const string CustomerOrAdmin = "CustomerOrAdmin";
}
