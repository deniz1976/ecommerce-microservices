using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalIdentityToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_provider",
                table: "users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_subject",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_external_provider_external_subject",
                table: "users",
                columns: new[] { "external_provider", "external_subject" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_external_provider_external_subject",
                table: "users");

            migrationBuilder.DropColumn(
                name: "external_provider",
                table: "users");

            migrationBuilder.DropColumn(
                name: "external_subject",
                table: "users");
        }
    }
}
