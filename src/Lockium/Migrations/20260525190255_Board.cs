using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lockium.Migrations
{
    public partial class Board : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BoardId",
                table: "DeviceLogs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BoardId",
                table: "Channels",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DeviceId = table.Column<long>(type: "bigint", nullable: true),
                    UpId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Boards_Boards_UpId",
                        column: x => x.UpId,
                        principalTable: "Boards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Boards_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IRChannels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<int>(type: "integer", nullable: false),
                    BoardId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IRChannels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IRChannels_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceLogs_BoardId",
                table: "DeviceLogs",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_BoardId",
                table: "Channels",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_Boards_DeviceId",
                table: "Boards",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Boards_UpId",
                table: "Boards",
                column: "UpId");

            migrationBuilder.CreateIndex(
                name: "IX_IRChannels_BoardId",
                table: "IRChannels",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Boards_BoardId",
                table: "Channels",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceLogs_Boards_BoardId",
                table: "DeviceLogs",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Boards_BoardId",
                table: "Channels");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceLogs_Boards_BoardId",
                table: "DeviceLogs");

            migrationBuilder.DropTable(
                name: "IRChannels");

            migrationBuilder.DropTable(
                name: "Boards");

            migrationBuilder.DropIndex(
                name: "IX_DeviceLogs_BoardId",
                table: "DeviceLogs");

            migrationBuilder.DropIndex(
                name: "IX_Channels_BoardId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "BoardId",
                table: "DeviceLogs");

            migrationBuilder.DropColumn(
                name: "BoardId",
                table: "Channels");
        }
    }
}
