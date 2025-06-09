using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mgr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    wallet_address = table.Column<string>(type: "text", nullable: false),
                    telegram = table.Column<string>(type: "text", nullable: true),
                    orders_count = table.Column<int>(type: "integer", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    last_active_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account", x => x.wallet_address);
                });

            migrationBuilder.CreateTable(
                name: "escrow_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    escrow_pda = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    deal_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    seller_crypto = table.Column<string>(type: "text", nullable: true),
                    buyer_fiat = table.Column<string>(type: "text", nullable: true),
                    token_mint = table.Column<string>(type: "text", nullable: true),
                    fiat_code = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    price = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    closed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    offer_side = table.Column<byte>(type: "smallint", nullable: false),
                    min_fiat_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    max_fiat_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    filled_quantity = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_escrow_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    deal_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    last_message_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.deal_id);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    account_id = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_account_account_id",
                        column: x => x.account_id,
                        principalTable: "account",
                        principalColumn: "wallet_address",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "deal_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_messages_account_id",
                table: "messages",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_room_id_created_at_utc",
                table: "messages",
                columns: new[] { "room_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_rooms_deal_id",
                table: "rooms",
                column: "deal_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "escrow_orders");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "rooms");
        }
    }
}
