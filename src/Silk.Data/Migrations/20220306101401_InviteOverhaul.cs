using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class InviteOverhaul : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE TABLE temp_invites AS SELECT \"Id\", \"match_aggressively\", \"infract_on_invite\", \"scan_invite_origin\", \"delete_invite_messages\", \"invite_whitelist_enabled\" FROM \"guild_moderation_config\"");
            migrationBuilder.DropForeignKey(
                name: "FK_guild_moderation_config_guild_logging_configs_LoggingConfig~",
                table: "guild_moderation_config");

            migrationBuilder.DropForeignKey(
                name: "FK_invites_guild_moderation_config_GuildModConfigEntityId",
                table: "invites");

            migrationBuilder.DropColumn(
                name: "delete_invite_messages",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "infract_on_invite",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "invite_whitelist_enabled",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "match_aggressively",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "scan_invite_origin",
                table: "guild_moderation_config");

            migrationBuilder.RenameColumn(
                name: "GuildModConfigEntityId",
                table: "invites",
                newName: "InviteConfigEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_invites_GuildModConfigEntityId",
                table: "invites",
                newName: "IX_invites_InviteConfigEntityId");

            migrationBuilder.RenameColumn(
                name: "LoggingConfigId",
                table: "guild_moderation_config",
                newName: "LoggingId");

            migrationBuilder.RenameIndex(
                name: "IX_guild_moderation_config_LoggingConfigId",
                table: "guild_moderation_config",
                newName: "IX_guild_moderation_config_LoggingId");

            migrationBuilder.CreateTable(
                name: "invite_configs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    whitelist_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    match_aggressively = table.Column<bool>(type: "boolean", nullable: false),
                    infract = table.Column<bool>(type: "boolean", nullable: false),
                    delete = table.Column<bool>(type: "boolean", nullable: false),
                    scan_origin = table.Column<bool>(type: "boolean", nullable: false),
                    GuildModConfigId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invite_configs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invite_configs_guild_moderation_config_GuildModConfigId",
                        column: x => x.GuildModConfigId,
                        principalTable: "guild_moderation_config",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_invite_configs_GuildModConfigId",
                table: "invite_configs",
                column: "GuildModConfigId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_moderation_config_guild_logging_configs_LoggingId",
                table: "guild_moderation_config",
                column: "LoggingId",
                principalTable: "guild_logging_configs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_invites_invite_configs_InviteConfigEntityId",
                table: "invites",
                column: "InviteConfigEntityId",
                principalTable: "invite_configs",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guild_moderation_config_guild_logging_configs_LoggingId",
                table: "guild_moderation_config");

            migrationBuilder.DropForeignKey(
                name: "FK_invites_invite_configs_InviteConfigEntityId",
                table: "invites");

            migrationBuilder.DropTable(
                name: "invite_configs");

            migrationBuilder.RenameColumn(
                name: "InviteConfigEntityId",
                table: "invites",
                newName: "GuildModConfigEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_invites_InviteConfigEntityId",
                table: "invites",
                newName: "IX_invites_GuildModConfigEntityId");

            migrationBuilder.RenameColumn(
                name: "LoggingId",
                table: "guild_moderation_config",
                newName: "LoggingConfigId");

            migrationBuilder.RenameIndex(
                name: "IX_guild_moderation_config_LoggingId",
                table: "guild_moderation_config",
                newName: "IX_guild_moderation_config_LoggingConfigId");

            migrationBuilder.AddColumn<bool>(
                name: "delete_invite_messages",
                table: "guild_moderation_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "infract_on_invite",
                table: "guild_moderation_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "invite_whitelist_enabled",
                table: "guild_moderation_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "match_aggressively",
                table: "guild_moderation_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "scan_invite_origin",
                table: "guild_moderation_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_moderation_config_guild_logging_configs_LoggingConfig~",
                table: "guild_moderation_config",
                column: "LoggingConfigId",
                principalTable: "guild_logging_configs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_invites_guild_moderation_config_GuildModConfigEntityId",
                table: "invites",
                column: "GuildModConfigEntityId",
                principalTable: "guild_moderation_config",
                principalColumn: "Id");
        }
    }
}
