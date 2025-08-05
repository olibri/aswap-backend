using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PMethodManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "escrow_order_payment_methods",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    method_id = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_escrow_order_payment_methods", x => new { x.order_id, x.method_id });
                    table.ForeignKey(
                        name: "FK_escrow_order_payment_methods_escrow_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "escrow_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_escrow_order_payment_methods_payment_methods_method_id",
                        column: x => x.method_id,
                        principalTable: "payment_methods",
                        principalColumn: "method_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_escrow_best_price",
                table: "escrow_orders",
                columns: new[] { "token_mint", "fiat_code", "offer_side", "status", "price" });

            migrationBuilder.CreateIndex(
                name: "IX_escrow_order_payment_methods_method_id_order_id",
                table: "escrow_order_payment_methods",
                columns: new[] { "method_id", "order_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "escrow_order_payment_methods");

            migrationBuilder.DropIndex(
                name: "ix_escrow_best_price",
                table: "escrow_orders");

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
    }
}
