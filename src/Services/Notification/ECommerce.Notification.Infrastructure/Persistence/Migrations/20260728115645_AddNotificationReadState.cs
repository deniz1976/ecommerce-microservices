using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Notification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationReadState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "read_at",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_customer_id_read_at_created_at",
                table: "notifications",
                columns: new[] { "customer_id", "read_at", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notifications_customer_id_read_at_created_at",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "read_at",
                table: "notifications");
        }
    }
}
