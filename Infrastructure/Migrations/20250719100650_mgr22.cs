using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mgr22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_order_status_daily",
                table: "order_status_daily");

            migrationBuilder.AddPrimaryKey(
                name: "PK_order_status_daily",
                table: "order_status_daily",
                columns: new[] { "day", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_order_status_daily",
                table: "order_status_daily");

            migrationBuilder.AddPrimaryKey(
                name: "PK_order_status_daily",
                table: "order_status_daily",
                column: "day");
        }
    }
}
