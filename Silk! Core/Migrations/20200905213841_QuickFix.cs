using Microsoft.EntityFrameworkCore.Migrations;

namespace SilkBot.Migrations
{
    public partial class QuickFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ban_DiscordUserInfoSet_UserInfoId",
                table: "Ban");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscordUserInfoSet_Guilds_GuildId",
                table: "DiscordUserInfoSet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscordUserInfoSet",
                table: "DiscordUserInfoSet");

            migrationBuilder.RenameTable(
                name: "DiscordUserInfoSet",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_DiscordUserInfoSet_GuildId",
                table: "Users",
                newName: "IX_Users_GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ban_Users_UserInfoId",
                table: "Ban",
                column: "UserInfoId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ban_Users_UserInfoId",
                table: "Ban");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "DiscordUserInfoSet");

            migrationBuilder.RenameIndex(
                name: "IX_Users_GuildId",
                table: "DiscordUserInfoSet",
                newName: "IX_DiscordUserInfoSet_GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscordUserInfoSet",
                table: "DiscordUserInfoSet",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ban_DiscordUserInfoSet_UserInfoId",
                table: "Ban",
                column: "UserInfoId",
                principalTable: "DiscordUserInfoSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordUserInfoSet_Guilds_GuildId",
                table: "DiscordUserInfoSet",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
