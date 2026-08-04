namespace ECommerce.Catalog.Application.Stores;

public sealed record StoreAccessContext(Guid? UserId, bool IsAdmin);
