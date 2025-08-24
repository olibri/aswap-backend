using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class applock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_lock",
                columns: table => new
                {
                    name = table.Column<string>(type: "text", nullable: false),
                    locked_until_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lock_owner = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_lock", x => x.name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_lock_locked_until_utc",
                table: "app_lock",
                column: "locked_until_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_lock");
        }
    }
}
