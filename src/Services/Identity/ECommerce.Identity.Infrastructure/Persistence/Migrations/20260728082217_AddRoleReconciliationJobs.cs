using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleReconciliationJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "role_reconciliation_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    desired_role = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    current_external_role = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_reconciliation_jobs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_role_reconciliation_jobs_completed_at_next_attempt_at",
                table: "role_reconciliation_jobs",
                columns: new[] { "completed_at", "next_attempt_at" });

            migrationBuilder.CreateIndex(
                name: "IX_role_reconciliation_jobs_external_subject_desired_role",
                table: "role_reconciliation_jobs",
                columns: new[] { "external_subject", "desired_role" },
                unique: true,
                filter: "completed_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_reconciliation_jobs");
        }
    }
}
