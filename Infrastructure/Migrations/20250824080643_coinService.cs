using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class coinService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "price_snapshot_minute",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token_mint = table.Column<string>(type: "text", nullable: false),
                    quote = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    price = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    minute_bucket_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    collected_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_snapshot_minute", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "token",
                columns: table => new
                {
                    mint = table.Column<string>(type: "text", nullable: false),
                    symbol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    decimals = table.Column<int>(type: "integer", nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_token", x => x.mint);
                });

            migrationBuilder.CreateIndex(
                name: "ix_price_minute_bucket",
                table: "price_snapshot_minute",
                column: "minute_bucket_utc");

            migrationBuilder.CreateIndex(
                name: "ix_price_token_quote",
                table: "price_snapshot_minute",
                columns: new[] { "token_mint", "quote" });

            migrationBuilder.CreateIndex(
                name: "ux_price_minute",
                table: "price_snapshot_minute",
                columns: new[] { "token_mint", "quote", "minute_bucket_utc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_token_symbol",
                table: "token",
                column: "symbol");

            migrationBuilder.CreateIndex(
                name: "ix_token_verified",
                table: "token",
                column: "is_verified");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_snapshot_minute");

            migrationBuilder.DropTable(
                name: "token");
        }
    }
}
