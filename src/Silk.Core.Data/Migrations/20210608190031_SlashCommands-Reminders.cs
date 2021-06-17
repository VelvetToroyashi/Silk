using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class SlashCommandsReminders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_Users_OwnerId_GuildId",
                table: "Reminders");

            migrationBuilder.DropIndex(
                name: "IX_Reminders_OwnerId_GuildId",
                table: "Reminders");

            migrationBuilder.AlterColumn<string>(
                name: "MessageContent",
                table: "Reminders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "Reminders",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

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

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "MessageContent",
                table: "Reminders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "Reminders",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_OwnerId_GuildId",
                table: "Reminders",
                columns: new[] { "OwnerId", "GuildId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_Users_OwnerId_GuildId",
                table: "Reminders",
                columns: new[] { "OwnerId", "GuildId" },
                principalTable: "Users",
                principalColumns: new[] { "Id", "GuildId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
