using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class gg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_swap_history",
                columns: table => new
                {
                    tx = table.Column<string>(type: "text", nullable: false),
                    crypto_from = table.Column<string>(type: "text", nullable: false),
                    crypto_to = table.Column<string>(type: "text", nullable: false),
                    amount_in = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    amount_out = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    price_usd = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_swap_history", x => x.tx);
                });

            migrationBuilder.CreateIndex(
                name: "ix_swap_created_at",
                table: "account_swap_history",
                column: "created_at_utc");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_swap_history");
        }
    }
}
