namespace ECommerce.Identity.Domain;

public static class UserRoleNames
{
    public const string Admin = "Admin";
    public const string Seller = "Seller";
    public const string Customer = "Customer";

    public static bool IsSelfServiceRole(string role)
    {
        return string.Equals(role, Seller, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, Customer, StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeSelfServiceRole(string role)
    {
        if (string.Equals(role, Seller, StringComparison.OrdinalIgnoreCase))
        {
            return Seller;
        }

        if (string.Equals(role, Customer, StringComparison.OrdinalIgnoreCase))
        {
            return Customer;
        }

        throw new InvalidSelfServiceRoleException(role);
    }
}
