using Microsoft.EntityFrameworkCore.Migrations;

namespace SilkBot.Migrations
{
    public partial class dbFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "Users",
                newName: "Guilds");

            migrationBuilder.RenameIndex(
                name: "IX_Users_GuildId",
                table: "Users",
                newName: "IX_Users_Guilds");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Guilds_Guilds",
                table: "Users",
                column: "Guilds",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Guilds_Guilds",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Guilds",
                table: "Users",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Guilds",
                table: "Users",
                newName: "IX_Users_GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
