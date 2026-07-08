using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lockium.Migrations
{
    public partial class Billing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rate",
                table: "Billings");

            migrationBuilder.DropColumn(
                name: "TimeUnit",
                table: "Billings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Rate",
                table: "Billings",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TimeUnit",
                table: "Billings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
