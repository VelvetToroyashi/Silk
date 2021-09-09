using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Api.Data.Migrations
{
    public partial class InfractionUserFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddedByDiscordId",
                table: "Infractions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_AddedByDiscordId",
                table: "Infractions",
                column: "AddedByDiscordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_Users_AddedByDiscordId",
                table: "Infractions",
                column: "AddedByDiscordId",
                principalTable: "Users",
                principalColumn: "DiscordId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_Users_AddedByDiscordId",
                table: "Infractions");

            migrationBuilder.DropIndex(
                name: "IX_Infractions_AddedByDiscordId",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "AddedByDiscordId",
                table: "Infractions");
        }
    }
}
