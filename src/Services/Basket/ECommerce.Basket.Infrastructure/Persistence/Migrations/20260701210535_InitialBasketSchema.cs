using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Basket.Infrastructure.Persistence.Migrations
{
    public partial class InitialBasketSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "basket_checkout_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_basket_checkout_snapshots", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "basket_checkout_snapshot_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    basket_checkout_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_basket_checkout_snapshot_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_basket_checkout_snapshot_items_basket_checkout_snapshots_ba~",
                        column: x => x.basket_checkout_snapshot_id,
                        principalTable: "basket_checkout_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_basket_checkout_snapshot_items_basket_checkout_snapshot_id",
                table: "basket_checkout_snapshot_items",
                column: "basket_checkout_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_basket_checkout_snapshot_items_product_id",
                table: "basket_checkout_snapshot_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_basket_checkout_snapshots_created_at",
                table: "basket_checkout_snapshots",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_basket_checkout_snapshots_customer_id",
                table: "basket_checkout_snapshots",
                column: "customer_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "basket_checkout_snapshot_items");

            migrationBuilder.DropTable(
                name: "basket_checkout_snapshots");
        }
    }
}
