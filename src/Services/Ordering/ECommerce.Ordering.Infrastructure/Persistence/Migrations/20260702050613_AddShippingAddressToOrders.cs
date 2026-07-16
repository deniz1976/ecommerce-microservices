using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Ordering.Infrastructure.Persistence.Migrations
{
    public partial class AddShippingAddressToOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address_line",
                table: "orders",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "orders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "orders",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "postal_code",
                table: "orders",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "recipient_name",
                table: "orders",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address_line",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "city",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "postal_code",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "recipient_name",
                table: "orders");
        }
    }
}
