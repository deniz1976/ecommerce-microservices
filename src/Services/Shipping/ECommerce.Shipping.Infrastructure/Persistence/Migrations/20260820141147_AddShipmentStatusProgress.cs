using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Shipping.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentStatusProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "last_status_update_id",
                table: "shipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "status_updated_at",
                table: "shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "version",
                table: "shipments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_status_update_id",
                table: "shipments");

            migrationBuilder.DropColumn(
                name: "status_updated_at",
                table: "shipments");

            migrationBuilder.DropColumn(
                name: "version",
                table: "shipments");
        }
    }
}
