using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class paymentMInOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "payment_method_id",
                table: "escrow_orders",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_escrow_best_price_method",
                table: "escrow_orders",
                columns: new[] { "token_mint", "fiat_code", "offer_side", "status", "payment_method_id", "price" });

            migrationBuilder.CreateIndex(
                name: "IX_escrow_orders_payment_method_id",
                table: "escrow_orders",
                column: "payment_method_id");

            migrationBuilder.AddForeignKey(
                name: "FK_escrow_orders_payment_methods_payment_method_id",
                table: "escrow_orders",
                column: "payment_method_id",
                principalTable: "payment_methods",
                principalColumn: "method_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_escrow_orders_payment_methods_payment_method_id",
                table: "escrow_orders");

            migrationBuilder.DropIndex(
                name: "ix_escrow_best_price_method",
                table: "escrow_orders");

            migrationBuilder.DropIndex(
                name: "IX_escrow_orders_payment_method_id",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "payment_method_id",
                table: "escrow_orders");
        }
    }
}
