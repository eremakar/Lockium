using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lockium.Migrations
{
    public partial class Order_CellId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CellId",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CellId",
                table: "Orders",
                column: "CellId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Cells_CellId",
                table: "Orders",
                column: "CellId",
                principalTable: "Cells",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Cells_CellId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CellId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CellId",
                table: "Orders");
        }
    }
}
