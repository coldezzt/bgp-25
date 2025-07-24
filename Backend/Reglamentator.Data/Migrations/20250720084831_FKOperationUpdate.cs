using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reglamentator.Data.Migrations
{
    /// <inheritdoc />
    public partial class FKOperationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_operations_telegram_users_telegram_user_id",
                table: "operations");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_telegram_users_telegram_id",
                table: "telegram_users",
                column: "telegram_id");

            migrationBuilder.AddForeignKey(
                name: "fk_operations_telegram_users_telegram_user_id",
                table: "operations",
                column: "telegram_user_id",
                principalTable: "telegram_users",
                principalColumn: "telegram_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_operations_telegram_users_telegram_user_id",
                table: "operations");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_telegram_users_telegram_id",
                table: "telegram_users");

            migrationBuilder.AddForeignKey(
                name: "fk_operations_telegram_users_telegram_user_id",
                table: "operations",
                column: "telegram_user_id",
                principalTable: "telegram_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
