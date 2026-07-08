using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lockium.Migrations
{
    public partial class Reservation_CellId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CellId",
                table: "Reservations",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CellId",
                table: "Reservations",
                column: "CellId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Cells_CellId",
                table: "Reservations",
                column: "CellId",
                principalTable: "Cells",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Cells_CellId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CellId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CellId",
                table: "Reservations");
        }
    }
}
