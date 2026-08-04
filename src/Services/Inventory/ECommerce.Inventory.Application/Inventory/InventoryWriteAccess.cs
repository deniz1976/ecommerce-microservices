namespace ECommerce.Inventory.Application.Inventory;

public sealed class InventoryWriteAccess
{
    public InventoryWriteAccess(
        bool bypassProductOwnership,
        string? accessToken)
    {
        BypassProductOwnership = bypassProductOwnership;
        AccessToken = accessToken;
    }

    public bool BypassProductOwnership { get; }

    public string? AccessToken { get; }

    public override string ToString()
    {
        return $"{nameof(InventoryWriteAccess)} {{ " +
            $"{nameof(BypassProductOwnership)} = {BypassProductOwnership}, " +
            $"{nameof(AccessToken)} = [REDACTED] }}";
    }
}
