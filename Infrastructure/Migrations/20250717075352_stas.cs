using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class stas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aggregator_state",
                columns: table => new
                {
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aggregator_state", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "order_created_daily",
                columns: table => new
                {
                    day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    side = table.Column<byte>(type: "smallint", nullable: false),
                    created_cnt = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_created_daily", x => new { x.day, x.side });
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_created_daily_day",
                table: "order_created_daily",
                column: "day");

            migrationBuilder.CreateIndex(
                name: "IX_order_created_daily_side",
                table: "order_created_daily",
                column: "side");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aggregator_state");

            migrationBuilder.DropTable(
                name: "order_created_daily");
        }
    }
}
