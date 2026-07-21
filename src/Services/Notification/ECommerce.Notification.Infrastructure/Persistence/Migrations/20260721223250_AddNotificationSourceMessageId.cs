using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Notification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSourceMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_message_id",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE notifications
                SET source_message_id = gen_random_uuid()
                WHERE source_message_id IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "source_message_id",
                table: "notifications",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_source_message_id_channel",
                table: "notifications",
                columns: new[] { "source_message_id", "channel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notifications_source_message_id_channel",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "source_message_id",
                table: "notifications");
        }
    }
}
