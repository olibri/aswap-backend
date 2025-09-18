using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class refferal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "referral_code",
                table: "account",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "referral_count",
                table: "account",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "referral_earnings_usd",
                table: "account",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "referred_by",
                table: "account",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "referral_rewards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    referrer_wallet = table.Column<string>(type: "text", nullable: false),
                    referee_wallet = table.Column<string>(type: "text", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reward_usd = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    reward_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    order_value_usd = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referral_rewards", x => x.id);
                    table.ForeignKey(
                        name: "FK_referral_rewards_account_referee_wallet",
                        column: x => x.referee_wallet,
                        principalTable: "account",
                        principalColumn: "wallet_address",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_referral_rewards_account_referrer_wallet",
                        column: x => x.referrer_wallet,
                        principalTable: "account",
                        principalColumn: "wallet_address",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_referral_rewards_escrow_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "escrow_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "referral_stats_daily",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    referrer_wallet = table.Column<string>(type: "text", nullable: false),
                    new_referrals = table.Column<int>(type: "integer", nullable: false),
                    total_referrals = table.Column<int>(type: "integer", nullable: false),
                    rewards_earned_usd = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    orders_by_referrals = table.Column<int>(type: "integer", nullable: false),
                    volume_by_referrals_usd = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referral_stats_daily", x => new { x.day, x.referrer_wallet });
                    table.ForeignKey(
                        name: "FK_referral_stats_daily_account_referrer_wallet",
                        column: x => x.referrer_wallet,
                        principalTable: "account",
                        principalColumn: "wallet_address",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_referred_by",
                table: "account",
                column: "referred_by");

            migrationBuilder.CreateIndex(
                name: "ux_account_referral_code",
                table: "account",
                column: "referral_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_referral_rewards_processed_created",
                table: "referral_rewards",
                columns: new[] { "processed_at", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_referral_rewards_referee_created",
                table: "referral_rewards",
                columns: new[] { "referee_wallet", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_referral_rewards_referrer_created",
                table: "referral_rewards",
                columns: new[] { "referrer_wallet", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_referral_rewards_order",
                table: "referral_rewards",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_referral_stats_day_rewards",
                table: "referral_stats_daily",
                columns: new[] { "day", "rewards_earned_usd" });

            migrationBuilder.CreateIndex(
                name: "ix_referral_stats_referrer_day",
                table: "referral_stats_daily",
                columns: new[] { "referrer_wallet", "day" });

            migrationBuilder.CreateIndex(
                name: "ix_referral_stats_total_day",
                table: "referral_stats_daily",
                columns: new[] { "total_referrals", "day" });

            migrationBuilder.AddForeignKey(
                name: "FK_account_account_referred_by",
                table: "account",
                column: "referred_by",
                principalTable: "account",
                principalColumn: "wallet_address",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_account_account_referred_by",
                table: "account");

            migrationBuilder.DropTable(
                name: "referral_rewards");

            migrationBuilder.DropTable(
                name: "referral_stats_daily");

            migrationBuilder.DropIndex(
                name: "ix_account_referred_by",
                table: "account");

            migrationBuilder.DropIndex(
                name: "ux_account_referral_code",
                table: "account");

            migrationBuilder.DropColumn(
                name: "referral_code",
                table: "account");

            migrationBuilder.DropColumn(
                name: "referral_count",
                table: "account");

            migrationBuilder.DropColumn(
                name: "referral_earnings_usd",
                table: "account");

            migrationBuilder.DropColumn(
                name: "referred_by",
                table: "account");
        }
    }
}
