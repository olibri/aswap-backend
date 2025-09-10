using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class realesedTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "crypto_released_at",
                table: "escrow_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "payment_confirmed_at",
                table: "escrow_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "release_time_seconds",
                table: "escrow_orders",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "crypto_released_at",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "payment_confirmed_at",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "release_time_seconds",
                table: "escrow_orders");
        }
    }
}
