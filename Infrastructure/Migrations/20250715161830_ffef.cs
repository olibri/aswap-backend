using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ffef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asset_volume_daily",
                columns: table => new
                {
                    day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    token_mint = table.Column<string>(type: "text", nullable: false),
                    volume = table.Column<decimal>(type: "numeric(38,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_volume_daily", x => new { x.day, x.token_mint });
                });

            migrationBuilder.CreateTable(
                name: "bans",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wallet = table.Column<string>(type: "text", nullable: true),
                    telegram_id = table.Column<string>(type: "text", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: false),
                    banned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ts = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    wallet = table.Column<string>(type: "text", nullable: true),
                    ip = table.Column<string>(type: "text", nullable: true),
                    payload = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "funnel_metrics_daily",
                columns: table => new
                {
                    day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    connects_cnt = table.Column<int>(type: "integer", nullable: false),
                    orders_cnt = table.Column<int>(type: "integer", nullable: false),
                    trades_cnt = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funnel_metrics_daily", x => x.day);
                });

            migrationBuilder.CreateTable(
                name: "order_status_daily",
                columns: table => new
                {
                    day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    open_cnt = table.Column<int>(type: "integer", nullable: false),
                    filled_cnt = table.Column<int>(type: "integer", nullable: false),
                    cancelled_cnt = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_status_daily", x => x.day);
                });

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

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet = table.Column<string>(type: "text", nullable: true),
                    ip = table.Column<string>(type: "text", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.session_id);
                });

            migrationBuilder.CreateTable(
                name: "tvl_snapshots",
                columns: table => new
                {
                    taken_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    token_mint = table.Column<string>(type: "text", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(38,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tvl_snapshots", x => new { x.taken_at, x.token_mint });
                });

            migrationBuilder.CreateTable(
                name: "tx_history",
                columns: table => new
                {
                    tx_hash = table.Column<string>(type: "text", nullable: false),
                    wallet = table.Column<string>(type: "text", nullable: true),
                    token_mint = table.Column<string>(type: "text", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(38,0)", nullable: false),
                    side = table.Column<string>(type: "text", nullable: false),
                    ts = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tx_history", x => x.tx_hash);
                });

            migrationBuilder.CreateTable(
                name: "user_metrics_daily",
                columns: table => new
                {
                    day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dau_users = table.Column<int>(type: "integer", nullable: false),
                    dau_ips = table.Column<int>(type: "integer", nullable: false),
                    wau_users = table.Column<int>(type: "integer", nullable: false),
                    mau_users = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_metrics_daily", x => x.day);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_last_seen_at",
                table: "sessions",
                column: "last_seen_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asset_volume_daily");

            migrationBuilder.DropTable(
                name: "bans");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "funnel_metrics_daily");

            migrationBuilder.DropTable(
                name: "order_status_daily");

            migrationBuilder.DropTable(
                name: "ratings");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "tvl_snapshots");

            migrationBuilder.DropTable(
                name: "tx_history");

            migrationBuilder.DropTable(
                name: "user_metrics_daily");
        }
    }
}
