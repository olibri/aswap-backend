using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class gg1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_swap_from_date",
                table: "account_swap_history");

            migrationBuilder.DropIndex(
                name: "ix_swap_price_date",
                table: "account_swap_history");

            migrationBuilder.DropIndex(
                name: "ix_swap_to_date",
                table: "account_swap_history");

            migrationBuilder.RenameColumn(
                name: "price_usd",
                table: "account_swap_history",
                newName: "price_usd_out");

            migrationBuilder.AddColumn<decimal>(
                name: "price_usd_in",
                table: "account_swap_history",
                type: "numeric(38,18)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "ix_swap_from_date",
                table: "account_swap_history",
                columns: new[] { "crypto_from", "created_at_utc" })
                .Annotation("Npgsql:IndexInclude", new[] { "price_usd_in", "amount_in", "tx" });

            migrationBuilder.CreateIndex(
                name: "ix_swap_from_price_date",
                table: "account_swap_history",
                columns: new[] { "crypto_from", "price_usd_in", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_swap_to_date",
                table: "account_swap_history",
                columns: new[] { "crypto_to", "created_at_utc" })
                .Annotation("Npgsql:IndexInclude", new[] { "price_usd_out", "amount_out", "tx" });

            migrationBuilder.CreateIndex(
                name: "ix_swap_to_price_date",
                table: "account_swap_history",
                columns: new[] { "crypto_to", "price_usd_out", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_swap_from_date",
                table: "account_swap_history");

            migrationBuilder.DropIndex(
                name: "ix_swap_from_price_date",
                table: "account_swap_history");

            migrationBuilder.DropIndex(
                name: "ix_swap_to_date",
                table: "account_swap_history");

            migrationBuilder.DropIndex(
                name: "ix_swap_to_price_date",
                table: "account_swap_history");

            migrationBuilder.DropColumn(
                name: "price_usd_in",
                table: "account_swap_history");

            migrationBuilder.RenameColumn(
                name: "price_usd_out",
                table: "account_swap_history",
                newName: "price_usd");

            migrationBuilder.CreateIndex(
                name: "ix_swap_from_date",
                table: "account_swap_history",
                columns: new[] { "crypto_from", "created_at_utc" })
                .Annotation("Npgsql:IndexInclude", new[] { "price_usd", "tx" });

            migrationBuilder.CreateIndex(
                name: "ix_swap_price_date",
                table: "account_swap_history",
                columns: new[] { "price_usd", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_swap_to_date",
                table: "account_swap_history",
                columns: new[] { "crypto_to", "created_at_utc" })
                .Annotation("Npgsql:IndexInclude", new[] { "price_usd", "tx" });
        }
    }
}
