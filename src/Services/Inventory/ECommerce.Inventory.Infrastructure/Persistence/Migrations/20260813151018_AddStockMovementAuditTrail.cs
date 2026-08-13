using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovementAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "concurrency_version",
                table: "inventory_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "stock_movements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    movement_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    quantity_on_hand_before = table.Column<int>(type: "integer", nullable: false),
                    quantity_on_hand_after = table.Column<int>(type: "integer", nullable: false),
                    reserved_quantity_before = table.Column<int>(type: "integer", nullable: false),
                    reserved_quantity_after = table.Column<int>(type: "integer", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reservation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_movements", x => x.id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO stock_movements (
                    id,
                    product_id,
                    movement_type,
                    quantity,
                    quantity_on_hand_before,
                    quantity_on_hand_after,
                    reserved_quantity_before,
                    reserved_quantity_after,
                    order_id,
                    reservation_id,
                    occurred_at)
                SELECT
                    gen_random_uuid(),
                    product_id,
                    'AuditBaseline',
                    0,
                    quantity_on_hand,
                    quantity_on_hand,
                    reserved_quantity,
                    reserved_quantity,
                    NULL,
                    NULL,
                    updated_at
                FROM inventory_items;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_movement_type_reservation_id",
                table: "stock_movements",
                columns: new[] { "movement_type", "reservation_id" },
                unique: true,
                filter: "reservation_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_order_id",
                table: "stock_movements",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_product_id_occurred_at",
                table: "stock_movements",
                columns: new[] { "product_id", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_movements");

            migrationBuilder.DropColumn(
                name: "concurrency_version",
                table: "inventory_items");
        }
    }
}
