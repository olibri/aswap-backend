using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncUuidPk12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "price_snapshot_minute",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    token_mint = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    minute_bucket_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    collected_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
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
                name: "ux_price_minute",
                table: "price_snapshot_minute",
                columns: new[] { "token_mint", "minute_bucket_utc" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_snapshot_minute");
        }
    }
}
