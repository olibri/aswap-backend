using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncUuidPk1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_snapshot_minute");

            migrationBuilder.AddColumn<string>(
                name: "icon",
                table: "token",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "icon",
                table: "token");

            migrationBuilder.CreateTable(
                name: "price_snapshot_minute",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    collected_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    minute_bucket_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    price = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    quote = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    token_mint = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_snapshot_minute", x => x.id);
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
        }
    }
}
