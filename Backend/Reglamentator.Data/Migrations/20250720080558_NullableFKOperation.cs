using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reglamentator.Data.Migrations
{
    /// <inheritdoc />
    public partial class NullableFKOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_operations_operation_instances_next_operation_instance_id",
                table: "operations");

            migrationBuilder.AlterColumn<long>(
                name: "next_operation_instance_id",
                table: "operations",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "fk_operations_operation_instances_next_operation_instance_id",
                table: "operations",
                column: "next_operation_instance_id",
                principalTable: "operation_instances",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_operations_operation_instances_next_operation_instance_id",
                table: "operations");

            migrationBuilder.AlterColumn<long>(
                name: "next_operation_instance_id",
                table: "operations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_operations_operation_instances_next_operation_instance_id",
                table: "operations",
                column: "next_operation_instance_id",
                principalTable: "operation_instances",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
