using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mgr1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "escrow_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    escrow_pda = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    tx_init_sig = table.Column<string>(type: "text", nullable: false),
                    deal_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    seller = table.Column<string>(type: "text", nullable: false),
                    buyer = table.Column<string>(type: "text", nullable: true),
                    token_mint = table.Column<string>(type: "text", nullable: false),
                    fiat_code = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    price = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    closed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_escrow_orders", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "escrow_orders");
        }
    }
}
