using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addchild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "child_order",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deal_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    order_owner_wallet = table.Column<string>(type: "text", nullable: false),
                    contra_agent_wallet = table.Column<string>(type: "text", nullable: false),
                    escrow_status = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    closed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    filled_amount = table.Column<int>(type: "integer", nullable: true),
                    fill_nonce = table.Column<string>(type: "text", nullable: true),
                    fill_pda = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_child_order", x => x.id);
                    table.ForeignKey(
                        name: "FK_child_order_escrow_orders_parent_order_id",
                        column: x => x.parent_order_id,
                        principalTable: "escrow_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_child_order_deal",
                table: "child_order",
                column: "deal_id");

            migrationBuilder.CreateIndex(
                name: "ix_child_order_parent",
                table: "child_order",
                column: "parent_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_child_order_status_created",
                table: "child_order",
                columns: new[] { "escrow_status", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "child_order");
        }
    }
}
