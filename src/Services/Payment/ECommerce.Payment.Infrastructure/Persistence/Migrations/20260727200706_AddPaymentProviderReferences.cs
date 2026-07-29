using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentProviderReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "provider_name",
                table: "payments",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider_payment_reference",
                table: "payments",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider_transaction_reference",
                table: "payment_transactions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "provider_name",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "provider_payment_reference",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "provider_transaction_reference",
                table: "payment_transactions");
        }
    }
}
