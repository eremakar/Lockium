using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lockium.Migrations
{
    public partial class Order_Reservation_Recipient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Recipient",
                table: "Reservations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recipient",
                table: "Orders",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Recipient",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Recipient",
                table: "Orders");
        }
    }
}
