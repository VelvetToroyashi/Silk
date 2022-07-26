using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class ConfigMerge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_infraction_exemptions_guild_moderation_config_GuildModConfi~",
                table: "infraction_exemptions");

            migrationBuilder.DropForeignKey(
                name: "FK_infraction_steps_guild_moderation_config_GuildModConfigEnti~",
                table: "infraction_steps");

            migrationBuilder.DropForeignKey(
                name: "FK_invite_configs_guild_moderation_config_GuildModConfigId",
                table: "invite_configs");
            
            migrationBuilder.DropIndex(
               name: "IX_invite_configs_GuildModConfigId",
               table: "invite_configs");
            
            migrationBuilder.Sql("CREATE TABLE temp AS SELECT * FROM guild_moderation_config;");

            migrationBuilder.DropTable(
                name: "guild_moderation_config");

            migrationBuilder.RenameColumn(
                name: "GuildModConfigEntityId",
                table: "infraction_steps",
                newName: "GuildConfigEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_infraction_steps_GuildModConfigEntityId",
                table: "infraction_steps",
                newName: "IX_infraction_steps_GuildConfigEntityId");

            migrationBuilder.RenameColumn(
                name: "GuildModConfigEntityId",
                table: "infraction_exemptions",
                newName: "GuildConfigEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_infraction_exemptions_GuildModConfigEntityId",
                table: "infraction_exemptions",
                newName: "IX_infraction_exemptions_GuildConfigEntityId");

            migrationBuilder.AddColumn<int>(
                name: "LoggingId",
                table: "guild_configs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NamedInfractionSteps",
                table: "guild_configs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ban_suspicious_usernames",
                table: "guild_configs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "delete_detected_phishing",
                table: "guild_configs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "detect_phishing",
                table: "guild_configs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "max_role_mentions",
                table: "guild_configs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "max_user_mentions",
                table: "guild_configs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<ulong>(
                name: "mute_role",
                table: "guild_configs",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<bool>(
                name: "progressive_infractions",
                table: "guild_configs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "use_native_mute",
                table: "guild_configs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql
            (
                "UPDATE guild_configs config "                                               +
                "SET \"LoggingId\" = temp.\"LoggingId\","                      +
                "\"NamedInfractionSteps\" = temp.\"NamedInfractionSteps\",\n"  +
                "ban_suspicious_usernames = temp.ban_suspicious_usernames,\n"  +
                "delete_detected_phishing = temp.delete_detected_phishing, \n" +
                "detect_phishing = temp.detect_phishing,\n"                    +
                "max_role_mentions = temp.max_role_mentions,\n"                +
                "max_user_mentions = temp.max_user_mentions,\n"                +
                "mute_role = temp.mute_role,\n"                                +
                "progressive_infractions = temp.progressive_infractions,\n"    +
                "use_native_mute = temp.use_native_mute\n"                     +
                "FROM temp\n"                                                                +
                "WHERE config.guild_id = temp.guild_id;"
            );
            
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_infraction_exemptions_GuildConfigEntityId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_infraction_steps_GuildConfigEntityId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_invite_configs_GuildModConfigId\";");
            
            migrationBuilder.Sql("UPDATE invite_configs ic SET \"GuildModConfigId\" = (SELECT gc.\"Id\" FROM guild_configs gc INNER JOIN temp t USING (guild_id) WHERE t.\"Id\" = ic.\"Id\" );");
            migrationBuilder.Sql("UPDATE infraction_steps istep SET \"GuildConfigEntityId\" = (SELECT gc.\"Id\" FROM guild_configs gc INNER JOIN temp t USING (guild_id) WHERE t.\"Id\" = istep.\"GuildConfigEntityId\" );");
            migrationBuilder.Sql("UPDATE infraction_exemptions iexempt SET \"GuildConfigEntityId\" = (SELECT gc.\"Id\" FROM guild_configs gc INNER JOIN temp t USING (guild_id) WHERE t.\"Id\" = iexempt.\"GuildConfigEntityId\" );");
            
            migrationBuilder.Sql("CREATE UNIQUE INDEX \"IX_infraction_exemptions_GuildConfigEntityId\" ON infraction_exemptions USING btree (\"GuildConfigEntityId\");");
            migrationBuilder.Sql("CREATE UNIQUE INDEX \"IX_infraction_steps_GuildConfigEntityId\" ON infraction_steps USING btree (\"GuildConfigEntityId\");");
            migrationBuilder.Sql("CREATE UNIQUE INDEX \"IX_invite_configs_GuildModConfigId\" ON invite_configs USING btree (\"GuildModConfigId\");");
            
            migrationBuilder.Sql("ALTER TABLE guild_greetings DROP CONSTRAINT IF EXISTS \"FK_guild_greetings_guild_configs_GuildConfigEntityId\", " +
                                 "ADD CONSTRAINT \"FK_guild_greetings_guild_configs_GuildConfigEntityId\" FOREIGN KEY (\"GuildConfigEntityId\") REFERENCES guild_configs(\"Id\") ON DELETE CASCADE;");
            
            migrationBuilder.Sql("DROP TABLE temp;");

            migrationBuilder.CreateIndex(
                name: "IX_guild_configs_LoggingId",
                table: "guild_configs",
                column: "LoggingId");

            migrationBuilder.AddForeignKey(
                name: "FK_guild_configs_guild_logging_configs_LoggingId",
                table: "guild_configs",
                column: "LoggingId",
                principalTable: "guild_logging_configs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_infraction_exemptions_guild_configs_GuildConfigEntityId",
                table: "infraction_exemptions",
                column: "GuildConfigEntityId",
                principalTable: "guild_configs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_infraction_steps_guild_configs_GuildConfigEntityId",
                table: "infraction_steps",
                column: "GuildConfigEntityId",
                principalTable: "guild_configs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_invite_configs_guild_configs_GuildModConfigId",
                table: "invite_configs",
                column: "GuildModConfigId",
                principalTable: "guild_configs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guild_configs_guild_logging_configs_LoggingId",
                table: "guild_configs");

            migrationBuilder.DropForeignKey(
                name: "FK_infraction_exemptions_guild_configs_GuildConfigEntityId",
                table: "infraction_exemptions");

            migrationBuilder.DropForeignKey(
                name: "FK_infraction_steps_guild_configs_GuildConfigEntityId",
                table: "infraction_steps");

            migrationBuilder.DropForeignKey(
                name: "FK_invite_configs_guild_configs_GuildModConfigId",
                table: "invite_configs");

            migrationBuilder.DropIndex(
                name: "IX_guild_configs_LoggingId",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "LoggingId",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "NamedInfractionSteps",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "ban_suspicious_usernames",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "delete_detected_phishing",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "detect_phishing",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "max_role_mentions",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "max_user_mentions",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "mute_role",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "progressive_infractions",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "use_native_mute",
                table: "guild_configs");

            migrationBuilder.RenameColumn(
                name: "GuildConfigEntityId",
                table: "infraction_steps",
                newName: "GuildModConfigEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_infraction_steps_GuildConfigEntityId",
                table: "infraction_steps",
                newName: "IX_infraction_steps_GuildModConfigEntityId");

            migrationBuilder.RenameColumn(
                name: "GuildConfigEntityId",
                table: "infraction_exemptions",
                newName: "GuildModConfigEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_infraction_exemptions_GuildConfigEntityId",
                table: "infraction_exemptions",
                newName: "IX_infraction_exemptions_GuildModConfigEntityId");

            migrationBuilder.CreateTable(
                name: "guild_moderation_config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LoggingId = table.Column<int>(type: "integer", nullable: false),
                    ban_suspicious_usernames = table.Column<bool>(type: "boolean", nullable: false),
                    delete_detected_phishing = table.Column<bool>(type: "boolean", nullable: false),
                    detect_phishing = table.Column<bool>(type: "boolean", nullable: false),
                    max_role_mentions = table.Column<int>(type: "integer", nullable: false),
                    max_user_mentions = table.Column<int>(type: "integer", nullable: false),
                    mute_role = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    NamedInfractionSteps = table.Column<string>(type: "text", nullable: false),
                    progressive_infractions = table.Column<bool>(type: "boolean", nullable: false),
                    use_native_mute = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_moderation_config", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guild_moderation_config_guild_logging_configs_LoggingId",
                        column: x => x.LoggingId,
                        principalTable: "guild_logging_configs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_guild_moderation_config_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_guild_moderation_config_guild_id",
                table: "guild_moderation_config",
                column: "guild_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_guild_moderation_config_LoggingId",
                table: "guild_moderation_config",
                column: "LoggingId");

            migrationBuilder.AddForeignKey(
                name: "FK_infraction_exemptions_guild_moderation_config_GuildModConfi~",
                table: "infraction_exemptions",
                column: "GuildModConfigEntityId",
                principalTable: "guild_moderation_config",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_infraction_steps_guild_moderation_config_GuildModConfigEnti~",
                table: "infraction_steps",
                column: "GuildModConfigEntityId",
                principalTable: "guild_moderation_config",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_invite_configs_guild_moderation_config_GuildModConfigId",
                table: "invite_configs",
                column: "GuildModConfigId",
                principalTable: "guild_moderation_config",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
