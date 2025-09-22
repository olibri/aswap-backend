using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "child_order");

            migrationBuilder.DropColumn(
                name: "buyer_fiat",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "seller_crypto",
                table: "escrow_orders");

            migrationBuilder.RenameColumn(
                name: "escrow_pda",
                table: "escrow_orders",
                newName: "order_pda");

            migrationBuilder.RenameColumn(
                name: "deal_id",
                table: "escrow_orders",
                newName: "order_id");

            migrationBuilder.AddColumn<string>(
                name: "acceptor_wallet",
                table: "escrow_orders",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "creator_wallet",
                table: "escrow_orders",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vault_pda",
                table: "escrow_orders",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "universal_tickets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_pda = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ticket_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    order_owner_wallet = table.Column<string>(type: "text", nullable: false),
                    contra_agent_wallet = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    closed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_universal_tickets", x => x.id);
                    table.ForeignKey(
                        name: "FK_universal_tickets_escrow_orders_parent_order_id",
                        column: x => x.parent_order_id,
                        principalTable: "escrow_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_child_order_deal",
                table: "universal_tickets",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_child_order_parent",
                table: "universal_tickets",
                column: "parent_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_child_order_status_created",
                table: "universal_tickets",
                columns: new[] { "status", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "universal_tickets");

            migrationBuilder.DropColumn(
                name: "acceptor_wallet",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "creator_wallet",
                table: "escrow_orders");

            migrationBuilder.DropColumn(
                name: "vault_pda",
                table: "escrow_orders");

            migrationBuilder.RenameColumn(
                name: "order_pda",
                table: "escrow_orders",
                newName: "escrow_pda");

            migrationBuilder.RenameColumn(
                name: "order_id",
                table: "escrow_orders",
                newName: "deal_id");

            migrationBuilder.AddColumn<string>(
                name: "buyer_fiat",
                table: "escrow_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "seller_crypto",
                table: "escrow_orders",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "child_order",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    closed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    contra_agent_wallet = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deal_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    escrow_status = table.Column<int>(type: "integer", nullable: false),
                    fill_nonce = table.Column<int>(type: "integer", nullable: true),
                    filled_amount = table.Column<int>(type: "integer", nullable: true),
                    order_owner_wallet = table.Column<string>(type: "text", nullable: false),
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
    }
}
