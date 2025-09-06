using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class gg4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "auto_reply",
                table: "escrow_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "base_price",
                table: "escrow_orders",
                type: "numeric(38,18)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "listing_mode",
                table: "escrow_orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "margin_percent",
                table: "escrow_orders",
                type: "numeric(38,18)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "min_account_age_days",
                table: "escrow_orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payment_window_minutes",
                table: "escrow_orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "price_type",
                table: "escrow_orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "referral_code",
                table: "escrow_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "tags",
                table: "escrow_orders",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "terms",
                table: "escrow_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "visible_countries",
                table: "escrow_orders",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "auto_reply",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "base_price",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "listing_mode",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "margin_percent",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "min_account_age_days",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "payment_window_minutes",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "price_type",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "referral_code",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "terms",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "visible_countries",
                table: "escrow_orders");
        }
    }
}
