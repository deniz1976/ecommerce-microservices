using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SecureProductImageConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_images_product_id_sort_order",
                table: "product_images");

            migrationBuilder.Sql(
                """
                WITH ordered_images AS (
                    SELECT id,
                           (ROW_NUMBER() OVER (
                               PARTITION BY product_id
                               ORDER BY sort_order, id) - 1)::integer AS normalized_sort_order
                    FROM product_images
                )
                UPDATE product_images AS image
                SET sort_order = ordered_images.normalized_sort_order
                FROM ordered_images
                WHERE image.id = ordered_images.id;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_product_images_product_id_sort_order",
                table: "product_images",
                columns: new[] { "product_id", "sort_order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_images_product_id_sort_order",
                table: "product_images");

            migrationBuilder.CreateIndex(
                name: "IX_product_images_product_id_sort_order",
                table: "product_images",
                columns: new[] { "product_id", "sort_order" });
        }
    }
}
