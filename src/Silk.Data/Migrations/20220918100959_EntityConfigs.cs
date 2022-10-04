using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class EntityConfigs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "command_invocations");

            migrationBuilder.RenameColumn(
                name: "TimezoneID",
                table: "users",
                newName: "timezone_id");

            migrationBuilder.RenameColumn(
                name: "ShareTimezone",
                table: "users",
                newName: "share_timezone");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "reminders",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "invites",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "match_aggressively",
                table: "invite_configs",
                newName: "use_aggressive_regex");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "infractions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "infraction_steps",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "infraction_duration",
                table: "infraction_steps",
                newName: "infration_duration");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "infraction_exemptions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "exempt_from",
                table: "infraction_exemptions",
                newName: "exemption_type");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "guilds",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Option",
                table: "guild_greetings",
                newName: "option");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "guild_greetings",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "guild_greetings",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "MetadataID",
                table: "guild_greetings",
                newName: "metadata_id");

            migrationBuilder.RenameColumn(
                name: "GuildID",
                table: "guild_greetings",
                newName: "guild_id");

            migrationBuilder.RenameColumn(
                name: "ChannelID",
                table: "guild_greetings",
                newName: "channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_guild_greetings_GuildID",
                table: "guild_greetings",
                newName: "IX_guild_greetings_guild_id");

            migrationBuilder.RenameColumn(
                name: "raid_threshold",
                table: "guild_configs",
                newName: "raid_detection_threshold");

            migrationBuilder.RenameColumn(
                name: "progressive_infractions",
                table: "guild_configs",
                newName: "progressive_striking");

            migrationBuilder.RenameColumn(
                name: "detect_phishing",
                table: "guild_configs",
                newName: "detect_phishing_links");

            migrationBuilder.RenameColumn(
                name: "delete_detected_phishing",
                table: "guild_configs",
                newName: "delete_phishing_links");

            migrationBuilder.AlterColumn<bool>(
                name: "user_notified",
                table: "infractions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "processed",
                table: "infractions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "escalated",
                table: "infractions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "active",
                table: "infractions",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddForeignKey(
                name: "FK_guild_greetings_guilds_guild_id",
                table: "guild_greetings",
                column: "guild_id",
                principalTable: "guilds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guild_greetings_guilds_guild_id",
                table: "guild_greetings");

            migrationBuilder.RenameColumn(
                name: "timezone_id",
                table: "users",
                newName: "TimezoneID");

            migrationBuilder.RenameColumn(
                name: "share_timezone",
                table: "users",
                newName: "ShareTimezone");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "reminders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "invites",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "use_aggressive_regex",
                table: "invite_configs",
                newName: "match_aggressively");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "infractions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "infraction_steps",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "infration_duration",
                table: "infraction_steps",
                newName: "infraction_duration");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "infraction_exemptions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "exemption_type",
                table: "infraction_exemptions",
                newName: "exempt_from");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "guilds",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "option",
                table: "guild_greetings",
                newName: "Option");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "guild_greetings",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "guild_greetings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "metadata_id",
                table: "guild_greetings",
                newName: "MetadataID");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "guild_greetings",
                newName: "GuildID");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "guild_greetings",
                newName: "ChannelID");

            migrationBuilder.RenameIndex(
                name: "IX_guild_greetings_guild_id",
                table: "guild_greetings",
                newName: "IX_guild_greetings_GuildID");

            migrationBuilder.RenameColumn(
                name: "raid_detection_threshold",
                table: "guild_configs",
                newName: "raid_threshold");

            migrationBuilder.RenameColumn(
                name: "progressive_striking",
                table: "guild_configs",
                newName: "progressive_infractions");

            migrationBuilder.RenameColumn(
                name: "detect_phishing_links",
                table: "guild_configs",
                newName: "detect_phishing");

            migrationBuilder.RenameColumn(
                name: "delete_phishing_links",
                table: "guild_configs",
                newName: "delete_detected_phishing");

            migrationBuilder.AlterColumn<bool>(
                name: "user_notified",
                table: "infractions",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "processed",
                table: "infractions",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "escalated",
                table: "infractions",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "active",
                table: "infractions",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.CreateTable(
                name: "command_invocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    command_name = table.Column<string>(type: "text", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_command_invocations", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_guild_greetings_guilds_GuildID",
                table: "guild_greetings",
                column: "GuildID",
                principalTable: "guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
