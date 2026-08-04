using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.UnitTests;

public sealed class InventoryWriteAccessTests
{
    [Fact]
    public void ToString_does_not_expose_the_forwarded_access_token()
    {
        const string token = "sensitive-seller-token";
        InventoryWriteAccess access = new(false, token);

        string rendered = access.ToString();

        Assert.DoesNotContain(token, rendered, StringComparison.Ordinal);
        Assert.Contains("[REDACTED]", rendered, StringComparison.Ordinal);
    }
}
