using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class payment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payment_categories",
                columns: table => new
                {
                    category_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_categories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "payment_popularity_daily",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    method_id = table.Column<short>(type: "smallint", nullable: false),
                    region = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    cnt = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_popularity_daily", x => new { x.day, x.method_id, x.region });
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    method_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    category_id = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_methods", x => x.method_id);
                    table.ForeignKey(
                        name: "FK_payment_methods_payment_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "payment_categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payment_categories_name",
                table: "payment_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_methods_category_id",
                table: "payment_methods",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_methods_code",
                table: "payment_methods",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_popularity_daily_region_day_cnt",
                table: "payment_popularity_daily",
                columns: new[] { "region", "day", "cnt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "payment_popularity_daily");

            migrationBuilder.DropTable(
                name: "payment_categories");
        }
    }
}
