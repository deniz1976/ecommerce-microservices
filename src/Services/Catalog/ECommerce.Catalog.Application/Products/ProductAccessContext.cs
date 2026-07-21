namespace ECommerce.Catalog.Application.Products;

public sealed record ProductAccessContext(Guid? UserId, bool IsAdmin);
