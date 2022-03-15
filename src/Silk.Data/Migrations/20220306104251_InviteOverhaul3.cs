using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class InviteOverhaul3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invite_configs_guild_moderation_config_GuildModConfigId",
                table: "invite_configs");

            migrationBuilder.AlterColumn<int>(
                name: "GuildModConfigId",
                table: "invite_configs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_invite_configs_guild_moderation_config_GuildModConfigId",
                table: "invite_configs",
                column: "GuildModConfigId",
                principalTable: "guild_moderation_config",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invite_configs_guild_moderation_config_GuildModConfigId",
                table: "invite_configs");

            migrationBuilder.AlterColumn<int>(
                name: "GuildModConfigId",
                table: "invite_configs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_invite_configs_guild_moderation_config_GuildModConfigId",
                table: "invite_configs",
                column: "GuildModConfigId",
                principalTable: "guild_moderation_config",
                principalColumn: "Id");
        }
    }
}
