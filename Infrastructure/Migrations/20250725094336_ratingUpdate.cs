using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ratingUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ratings");

            migrationBuilder.CreateTable(
                name: "rating_reviews",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    to_wallet = table.Column<string>(type: "text", nullable: false),
                    from_wallet = table.Column<string>(type: "text", nullable: false),
                    score = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    deal_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rating_reviews", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rating_reviews");

            migrationBuilder.CreateTable(
                name: "ratings",
                columns: table => new
                {
                    wallet = table.Column<string>(type: "text", nullable: false),
                    completed = table.Column<int>(type: "integer", nullable: false),
                    disputes = table.Column<int>(type: "integer", nullable: false),
                    positive = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ratings", x => x.wallet);
                });
        }
    }
}
