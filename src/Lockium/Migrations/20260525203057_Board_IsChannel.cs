using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lockium.Migrations
{
    public partial class Board_IsChannel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsChannel",
                table: "Boards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsIR",
                table: "Boards",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChannel",
                table: "Boards");

            migrationBuilder.DropColumn(
                name: "IsIR",
                table: "Boards");
        }
    }
}
