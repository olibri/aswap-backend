using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mgr122 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            mb.Sql(@"
DO
$$
BEGIN
    -- перевіряємо, чи колонка вже є
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name   = 'escrow_orders'
          AND column_name  = 'status'
    ) THEN
        -- якщо є - дропнемо, бо таблиця порожня
        ALTER TABLE escrow_orders DROP COLUMN status;
    END IF;

    -- додаємо заново як integer
    ALTER TABLE escrow_orders
      ADD COLUMN status integer NOT NULL DEFAULT 0;
END
$$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "escrow_orders",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
