using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class DeprecateObsoleteProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "disbaled_commands");

            migrationBuilder.DropColumn(
                name: "AutoDehoist",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "BlacklistWords",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "LogMemberJoins",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "LogMemberLeaves",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "LogMessageChanges",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "LoggingChannel",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "LoggingWebhookUrl",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "UseWebhookLogging",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "WebhookLoggingId",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "GreetingChannel",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "GreetingOption",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "GreetingText",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "VerificationRole",
                table: "guild_configs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoDehoist",
                table: "guild_moderation_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BlacklistWords",
                table: "guild_moderation_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LogMemberJoins",
                table: "guild_moderation_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LogMemberLeaves",
                table: "guild_moderation_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LogMessageChanges",
                table: "guild_moderation_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LoggingChannel",
                table: "guild_moderation_config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LoggingWebhookUrl",
                table: "guild_moderation_config",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseWebhookLogging",
                table: "guild_moderation_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "WebhookLoggingId",
                table: "guild_moderation_config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GreetingChannel",
                table: "guild_configs",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "GreetingOption",
                table: "guild_configs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GreetingText",
                table: "guild_configs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "VerificationRole",
                table: "guild_configs",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "disbaled_commands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CommandName = table.Column<string>(type: "text", nullable: false),
                    GuildConfigEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_disbaled_commands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_disbaled_commands_guild_configs_GuildConfigEntityId",
                        column: x => x.GuildConfigEntityId,
                        principalTable: "guild_configs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_disbaled_commands_guilds_GuildID",
                        column: x => x.GuildID,
                        principalTable: "guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_disbaled_commands_GuildConfigEntityId",
                table: "disbaled_commands",
                column: "GuildConfigEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_disbaled_commands_GuildID_CommandName",
                table: "disbaled_commands",
                columns: new[] { "GuildID", "CommandName" },
                unique: true);
        }
    }
}
