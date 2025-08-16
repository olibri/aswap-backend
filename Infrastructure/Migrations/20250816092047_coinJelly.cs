using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class coinJelly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "coin_jelly",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_wallet_address = table.Column<string>(type: "text", nullable: false),
                    crypto_currency = table.Column<string>(type: "text", nullable: false),
                    crypto_currency_chain = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coin_jelly", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "coin_jelly_account_history_entity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tx_id = table.Column<string>(type: "text", nullable: false),
                    crypto_send = table.Column<string>(type: "text", nullable: false),
                    crypto_get = table.Column<string>(type: "text", nullable: false),
                    amount_send = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    amount_get = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    fee_atomic = table.Column<decimal>(type: "numeric(78,0)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coin_jelly_account_history_entity", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_coin_jelly_currency_chain",
                table: "coin_jelly",
                columns: new[] { "crypto_currency", "crypto_currency_chain" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cj_hist_created_at",
                table: "coin_jelly_account_history_entity",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_cj_hist_status_date",
                table: "coin_jelly_account_history_entity",
                columns: new[] { "status", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_cj_hist_txid",
                table: "coin_jelly_account_history_entity",
                column: "tx_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coin_jelly");

            migrationBuilder.DropTable(
                name: "coin_jelly_account_history_entity");
        }
    }
}
