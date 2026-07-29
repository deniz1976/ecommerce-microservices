using ECommerce.BuildingBlocks.Persistence;
using Npgsql;

namespace ECommerce.RuntimeChecks.Probes;

internal sealed class PostgresCatalogProductFixture : ICatalogProductFixture
{
    private const string ConnectionName = "ConnectionStrings__CatalogDb";

    public async Task CreateAsync(
        Guid productId,
        string productName,
        decimal unitPrice,
        string currency,
        CancellationToken cancellationToken)
    {
        string suffix = productId.ToString("N");
        const string sql = """
            insert into categories (id, slug, is_active, created_at)
            values (@fixture_id, @category_slug, true, @created_at);

            insert into category_translations (category_id, language_code, name)
            values (@fixture_id, 'en', 'Runtime Checkout Category');

            insert into brands (id, name, slug, is_active, created_at)
            values (@fixture_id, 'Runtime Checkout Brand', @brand_slug, true, @created_at);

            insert into products (
                id,
                sku,
                category_id,
                brand_id,
                store_id,
                price,
                currency,
                status,
                created_at,
                updated_at)
            values (
                @fixture_id,
                @sku,
                @fixture_id,
                @fixture_id,
                null,
                @unit_price,
                @currency,
                'Active',
                @created_at,
                @created_at);

            insert into product_translations (product_id, language_code, name, description)
            values (@fixture_id, 'en', @product_name, 'Managed runtime checkout fixture');
            """;

        await using NpgsqlConnection connection = await OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using NpgsqlCommand command = new(sql, connection, transaction);
        command.Parameters.AddWithValue("fixture_id", productId);
        command.Parameters.AddWithValue("category_slug", $"runtime-category-{suffix}");
        command.Parameters.AddWithValue("brand_slug", $"runtime-brand-{suffix}");
        command.Parameters.AddWithValue("sku", $"RUNTIME-{suffix}");
        command.Parameters.AddWithValue("unit_price", unitPrice);
        command.Parameters.AddWithValue("currency", currency);
        command.Parameters.AddWithValue("product_name", productName);
        command.Parameters.AddWithValue("created_at", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        Console.WriteLine($"Catalog runtime fixture created: {productId}");
    }

    public async Task DeleteAsync(Guid productId, CancellationToken cancellationToken)
    {
        const string sql = """
            delete from products where id = @fixture_id;
            delete from brands where id = @fixture_id;
            delete from categories where id = @fixture_id;
            """;

        await using NpgsqlConnection connection = await OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using NpgsqlCommand command = new(sql, connection, transaction);
        command.Parameters.AddWithValue("fixture_id", productId);

        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        Console.WriteLine($"Catalog runtime fixture deleted: {productId}");
    }

    private static async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        string connectionString = Environment.GetEnvironmentVariable(ConnectionName)
            ?? throw new InvalidOperationException($"{ConnectionName} is not set.");

        NpgsqlConnection connection = new(PostgresConnectionString.Normalize(connectionString));
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
