using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class SlashCommandsReminders_Attempt2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_Users_UserId_UserGuildId",
                table: "Reminders");

            migrationBuilder.DropIndex(
                name: "IX_Reminders_UserId_UserGuildId",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "UserGuildId",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Reminders");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UserGuildId",
                table: "Reminders",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UserId",
                table: "Reminders",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserId_UserGuildId",
                table: "Reminders",
                columns: new[] { "UserId", "UserGuildId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_Users_UserId_UserGuildId",
                table: "Reminders",
                columns: new[] { "UserId", "UserGuildId" },
                principalTable: "Users",
                principalColumns: new[] { "Id", "GuildId" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
