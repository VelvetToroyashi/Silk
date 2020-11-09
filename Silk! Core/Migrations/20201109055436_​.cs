using Microsoft.EntityFrameworkCore.Migrations;

namespace SilkBot.Migrations
{
    public partial class _​ : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ban_Guilds_GuildDiscordGuildId",
                table: "Ban");

            migrationBuilder.DropForeignKey(
                name: "FK_BlackListedWord_Guilds_GuildDiscordGuildId",
                table: "BlackListedWord");

            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_Guilds_GuildModelDiscordGuildId",
                table: "SelfAssignableRole");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Guilds_GuildDiscordGuildId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WhiteListedLink_Guilds_GuildDiscordGuildId",
                table: "WhiteListedLink");

            migrationBuilder.RenameColumn(
                name: "GuildDiscordGuildId",
                table: "WhiteListedLink",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_WhiteListedLink_GuildDiscordGuildId",
                table: "WhiteListedLink",
                newName: "IX_WhiteListedLink_GuildId");

            migrationBuilder.RenameColumn(
                name: "GuildDiscordGuildId",
                table: "Users",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_GuildDiscordGuildId",
                table: "Users",
                newName: "IX_Users_GuildId");

            migrationBuilder.RenameColumn(
                name: "GuildModelDiscordGuildId",
                table: "SelfAssignableRole",
                newName: "GuildModelId");

            migrationBuilder.RenameIndex(
                name: "IX_SelfAssignableRole_GuildModelDiscordGuildId",
                table: "SelfAssignableRole",
                newName: "IX_SelfAssignableRole_GuildModelId");

            migrationBuilder.RenameColumn(
                name: "DiscordGuildId",
                table: "Guilds",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "GuildDiscordGuildId",
                table: "BlackListedWord",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_BlackListedWord_GuildDiscordGuildId",
                table: "BlackListedWord",
                newName: "IX_BlackListedWord_GuildId");

            migrationBuilder.RenameColumn(
                name: "GuildDiscordGuildId",
                table: "Ban",
                newName: "GuildId1");

            migrationBuilder.RenameIndex(
                name: "IX_Ban_GuildDiscordGuildId",
                table: "Ban",
                newName: "IX_Ban_GuildId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Ban_Guilds_GuildId1",
                table: "Ban",
                column: "GuildId1",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BlackListedWord_Guilds_GuildId",
                table: "BlackListedWord",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SelfAssignableRole_Guilds_GuildModelId",
                table: "SelfAssignableRole",
                column: "GuildModelId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WhiteListedLink_Guilds_GuildId",
                table: "WhiteListedLink",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ban_Guilds_GuildId1",
                table: "Ban");

            migrationBuilder.DropForeignKey(
                name: "FK_BlackListedWord_Guilds_GuildId",
                table: "BlackListedWord");

            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_Guilds_GuildModelId",
                table: "SelfAssignableRole");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WhiteListedLink_Guilds_GuildId",
                table: "WhiteListedLink");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "WhiteListedLink",
                newName: "GuildDiscordGuildId");

            migrationBuilder.RenameIndex(
                name: "IX_WhiteListedLink_GuildId",
                table: "WhiteListedLink",
                newName: "IX_WhiteListedLink_GuildDiscordGuildId");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "Users",
                newName: "GuildDiscordGuildId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_GuildId",
                table: "Users",
                newName: "IX_Users_GuildDiscordGuildId");

            migrationBuilder.RenameColumn(
                name: "GuildModelId",
                table: "SelfAssignableRole",
                newName: "GuildModelDiscordGuildId");

            migrationBuilder.RenameIndex(
                name: "IX_SelfAssignableRole_GuildModelId",
                table: "SelfAssignableRole",
                newName: "IX_SelfAssignableRole_GuildModelDiscordGuildId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Guilds",
                newName: "DiscordGuildId");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "BlackListedWord",
                newName: "GuildDiscordGuildId");

            migrationBuilder.RenameIndex(
                name: "IX_BlackListedWord_GuildId",
                table: "BlackListedWord",
                newName: "IX_BlackListedWord_GuildDiscordGuildId");

            migrationBuilder.RenameColumn(
                name: "GuildId1",
                table: "Ban",
                newName: "GuildDiscordGuildId");

            migrationBuilder.RenameIndex(
                name: "IX_Ban_GuildId1",
                table: "Ban",
                newName: "IX_Ban_GuildDiscordGuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ban_Guilds_GuildDiscordGuildId",
                table: "Ban",
                column: "GuildDiscordGuildId",
                principalTable: "Guilds",
                principalColumn: "DiscordGuildId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BlackListedWord_Guilds_GuildDiscordGuildId",
                table: "BlackListedWord",
                column: "GuildDiscordGuildId",
                principalTable: "Guilds",
                principalColumn: "DiscordGuildId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SelfAssignableRole_Guilds_GuildModelDiscordGuildId",
                table: "SelfAssignableRole",
                column: "GuildModelDiscordGuildId",
                principalTable: "Guilds",
                principalColumn: "DiscordGuildId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Guilds_GuildDiscordGuildId",
                table: "Users",
                column: "GuildDiscordGuildId",
                principalTable: "Guilds",
                principalColumn: "DiscordGuildId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WhiteListedLink_Guilds_GuildDiscordGuildId",
                table: "WhiteListedLink",
                column: "GuildDiscordGuildId",
                principalTable: "Guilds",
                principalColumn: "DiscordGuildId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
