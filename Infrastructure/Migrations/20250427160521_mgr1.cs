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
                name: "EscrowOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EscrowPda = table.Column<string>(type: "text", nullable: false),
                    TxInitSig = table.Column<string>(type: "text", nullable: false),
                    DealId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Seller = table.Column<string>(type: "text", nullable: false),
                    Buyer = table.Column<string>(type: "text", nullable: true),
                    TokenMint = table.Column<string>(type: "text", nullable: false),
                    FiatCode = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OnChainAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscrowOrders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EscrowOrders_Status",
                table: "EscrowOrders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EscrowOrders");
        }
    }
}
