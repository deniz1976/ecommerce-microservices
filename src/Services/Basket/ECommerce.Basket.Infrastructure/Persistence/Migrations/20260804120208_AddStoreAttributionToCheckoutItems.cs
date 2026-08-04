using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Basket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreAttributionToCheckoutItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "store_id",
                table: "basket_checkout_snapshot_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_basket_checkout_snapshot_items_store_id",
                table: "basket_checkout_snapshot_items",
                column: "store_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_basket_checkout_snapshot_items_store_id",
                table: "basket_checkout_snapshot_items");

            migrationBuilder.DropColumn(
                name: "store_id",
                table: "basket_checkout_snapshot_items");
        }
    }
}
