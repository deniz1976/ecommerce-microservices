namespace ECommerce.Basket.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class RedisCollection : ICollectionFixture<RedisFixture>
{
    public const string Name = "basket-redis";
}
