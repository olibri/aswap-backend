using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mgr3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "seller",
                table: "escrow_orders",
                newName: "seller_crypto");

            migrationBuilder.RenameColumn(
                name: "buyer",
                table: "escrow_orders",
                newName: "buyer_fiat");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "seller_crypto",
                table: "escrow_orders",
                newName: "seller");

            migrationBuilder.RenameColumn(
                name: "buyer_fiat",
                table: "escrow_orders",
                newName: "buyer");
        }
    }
}
