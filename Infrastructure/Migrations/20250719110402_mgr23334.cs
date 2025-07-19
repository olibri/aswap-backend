using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mgr23334 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deal_time_daily",
                columns: table => new
                {
                    Day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TokenMint = table.Column<string>(type: "text", nullable: false),
                    AvgSeconds = table.Column<double>(type: "double precision", nullable: false),
                    TradeCnt = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deal_time_daily", x => new { x.Day, x.TokenMint });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deal_time_daily");
        }
    }
}
