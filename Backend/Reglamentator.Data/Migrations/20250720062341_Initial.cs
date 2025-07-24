using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Reglamentator.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "telegram_users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    telegram_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_telegram_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operation_instances",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    operation_id = table.Column<long>(type: "bigint", nullable: false),
                    result = table.Column<string>(type: "text", nullable: true),
                    executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operation_instances", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    theme = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cron = table.Column<string>(type: "text", nullable: true),
                    telegram_user_id = table.Column<long>(type: "bigint", nullable: false),
                    next_operation_instance_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operations", x => x.id);
                    table.ForeignKey(
                        name: "fk_operations_operation_instances_next_operation_instance_id",
                        column: x => x.next_operation_instance_id,
                        principalTable: "operation_instances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_operations_telegram_users_telegram_user_id",
                        column: x => x.telegram_user_id,
                        principalTable: "telegram_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reminders",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    message_template = table.Column<string>(type: "text", nullable: false),
                    offset_before_execution = table.Column<TimeSpan>(type: "interval", nullable: false),
                    operation_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reminders", x => x.id);
                    table.ForeignKey(
                        name: "fk_reminders_operations_operation_id",
                        column: x => x.operation_id,
                        principalTable: "operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_operation_instances_operation_id",
                table: "operation_instances",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "ix_operations_next_operation_instance_id",
                table: "operations",
                column: "next_operation_instance_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_operations_telegram_user_id",
                table: "operations",
                column: "telegram_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_reminders_operation_id",
                table: "reminders",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "ix_telegram_users_telegram_id",
                table: "telegram_users",
                column: "telegram_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_operation_instances_operations_operation_id",
                table: "operation_instances",
                column: "operation_id",
                principalTable: "operations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_operation_instances_operations_operation_id",
                table: "operation_instances");

            migrationBuilder.DropTable(
                name: "reminders");

            migrationBuilder.DropTable(
                name: "operations");

            migrationBuilder.DropTable(
                name: "operation_instances");

            migrationBuilder.DropTable(
                name: "telegram_users");
        }
    }
}
