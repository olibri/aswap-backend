using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newJellyField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "crypto_currency",
                table: "coin_jelly",
                newName: "crypto_currency_name");

            migrationBuilder.AddColumn<string>(
                name: "crypto_currency_code",
                table: "coin_jelly",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "crypto_currency_code",
                table: "coin_jelly");

            migrationBuilder.RenameColumn(
                name: "crypto_currency_name",
                table: "coin_jelly",
                newName: "crypto_currency");
        }
    }
}
