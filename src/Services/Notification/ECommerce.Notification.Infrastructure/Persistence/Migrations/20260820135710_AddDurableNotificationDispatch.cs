using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Notification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDurableNotificationDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_dispatches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    delivered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_dispatches", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_dispatches_notifications_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_dispatches_notification_id_channel",
                table: "notification_dispatches",
                columns: new[] { "notification_id", "channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_dispatches_status_next_attempt_at",
                table: "notification_dispatches",
                columns: new[] { "status", "next_attempt_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_dispatches");
        }
    }
}
