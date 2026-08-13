using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.OrderingSaga.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDurableWorkflowTimeouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "concurrency_version",
                table: "order_workflows",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "correlation_id",
                table: "order_workflows",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "step_deadline_at",
                table: "order_workflows",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "timeout_handled_at",
                table: "order_workflows",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE order_workflows
                SET correlation_id = order_id,
                    step_deadline_at = CASE status
                        WHEN 'Submitted' THEN updated_at + INTERVAL '120 seconds'
                        WHEN 'InventoryReserved' THEN updated_at + INTERVAL '120 seconds'
                        WHEN 'PaymentAuthorized' THEN updated_at + INTERVAL '180 seconds'
                        ELSE NULL
                    END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_order_workflows_status_step_deadline_at",
                table: "order_workflows",
                columns: new[] { "status", "step_deadline_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_workflows_status_step_deadline_at",
                table: "order_workflows");

            migrationBuilder.DropColumn(
                name: "concurrency_version",
                table: "order_workflows");

            migrationBuilder.DropColumn(
                name: "correlation_id",
                table: "order_workflows");

            migrationBuilder.DropColumn(
                name: "step_deadline_at",
                table: "order_workflows");

            migrationBuilder.DropColumn(
                name: "timeout_handled_at",
                table: "order_workflows");
        }
    }
}
