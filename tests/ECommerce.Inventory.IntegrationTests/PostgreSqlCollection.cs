namespace ECommerce.Inventory.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "inventory-postgresql";
}
