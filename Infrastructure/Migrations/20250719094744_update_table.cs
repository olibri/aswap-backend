using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancelled_cnt",
                table: "order_status_daily");

            migrationBuilder.RenameColumn(
                name: "open_cnt",
                table: "order_status_daily",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "filled_cnt",
                table: "order_status_daily",
                newName: "cnt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "order_status_daily",
                newName: "open_cnt");

            migrationBuilder.RenameColumn(
                name: "cnt",
                table: "order_status_daily",
                newName: "filled_cnt");

            migrationBuilder.AddColumn<int>(
                name: "cancelled_cnt",
                table: "order_status_daily",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
