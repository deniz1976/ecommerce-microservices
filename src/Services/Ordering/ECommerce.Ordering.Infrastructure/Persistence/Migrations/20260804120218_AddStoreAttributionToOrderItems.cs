using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Ordering.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreAttributionToOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "store_id",
                table: "order_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_items_store_id_order_id",
                table: "order_items",
                columns: new[] { "store_id", "order_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_items_store_id_order_id",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "store_id",
                table: "order_items");
        }
    }
}
