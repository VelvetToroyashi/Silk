using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class SnowflakeIDsConversion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "command_invocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    command_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_command_invocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    prefix = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "logging_channels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    webhook_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    webhook_token = table.Column<string>(type: "text", nullable: false),
                    channel_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logging_channels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    owner_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    message_id = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    reply_content = table.Column<string>(type: "text", nullable: true),
                    reply_author_id = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    reply_message_id = table.Column<ulong>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "guild_configs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    GreetingOption = table.Column<int>(type: "integer", nullable: false),
                    VerificationRole = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GreetingChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GreetingText = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_configs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guild_configs_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guild_tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uses = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    owner_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    GuildEntityID = table.Column<ulong>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guild_tags_guild_tags_parent_id",
                        column: x => x.parent_id,
                        principalTable: "guild_tags",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_guild_tags_guilds_GuildEntityID",
                        column: x => x.GuildEntityID,
                        principalTable: "guilds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    flags = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => new { x.id, x.guild_id });
                    table.ForeignKey(
                        name: "FK_users_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guild_logging_configs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    log_message_edits = table.Column<bool>(type: "boolean", nullable: false),
                    log_message_deletes = table.Column<bool>(type: "boolean", nullable: false),
                    log_infractions = table.Column<bool>(type: "boolean", nullable: false),
                    log_member_joins = table.Column<bool>(type: "boolean", nullable: false),
                    log_member_leaves = table.Column<bool>(type: "boolean", nullable: false),
                    fallback_logging_channel = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    use_webhook_logging = table.Column<bool>(type: "boolean", nullable: false),
                    InfractionsId = table.Column<int>(type: "integer", nullable: true),
                    MessageEditsId = table.Column<int>(type: "integer", nullable: true),
                    MessageDeletesId = table.Column<int>(type: "integer", nullable: true),
                    MemberJoinsId = table.Column<int>(type: "integer", nullable: true),
                    MemberLeavesId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_logging_configs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guild_logging_configs_logging_channels_InfractionsId",
                        column: x => x.InfractionsId,
                        principalTable: "logging_channels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_guild_logging_configs_logging_channels_MemberJoinsId",
                        column: x => x.MemberJoinsId,
                        principalTable: "logging_channels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_guild_logging_configs_logging_channels_MemberLeavesId",
                        column: x => x.MemberLeavesId,
                        principalTable: "logging_channels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_guild_logging_configs_logging_channels_MessageDeletesId",
                        column: x => x.MessageDeletesId,
                        principalTable: "logging_channels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_guild_logging_configs_logging_channels_MessageEditsId",
                        column: x => x.MessageEditsId,
                        principalTable: "logging_channels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "disbaled_commands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommandName = table.Column<string>(type: "text", nullable: false),
                    GuildID = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "guild_greetings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildID = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Option = table.Column<int>(type: "integer", nullable: false),
                    ChannelID = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    MetadataID = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    GuildConfigEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_greetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guild_greetings_guild_configs_GuildConfigEntityId",
                        column: x => x.GuildConfigEntityId,
                        principalTable: "guild_configs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "infractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    target_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    enforcer_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    case_id = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    processed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    escalated = table.Column<bool>(type: "boolean", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_infractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_infractions_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_infractions_users_target_id_guild_id",
                        columns: x => new { x.target_id, x.guild_id },
                        principalTable: "users",
                        principalColumns: new[] { "id", "guild_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_histories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    initial_join_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    join_dates = table.Column<List<DateTimeOffset>>(type: "timestamp with time zone[]", nullable: false),
                    leave_dates = table.Column<List<DateTimeOffset>>(type: "timestamp with time zone[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_histories_users_user_id_guild_id",
                        columns: x => new { x.user_id, x.guild_id },
                        principalTable: "users",
                        principalColumns: new[] { "id", "guild_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guild_moderation_config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    mute_role = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    max_user_mentions = table.Column<int>(type: "integer", nullable: false),
                    max_role_mentions = table.Column<int>(type: "integer", nullable: false),
                    LoggingChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LogMessageChanges = table.Column<bool>(type: "boolean", nullable: false),
                    LogMemberJoins = table.Column<bool>(type: "boolean", nullable: false),
                    LogMemberLeaves = table.Column<bool>(type: "boolean", nullable: false),
                    invite_whitelist_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    BlacklistWords = table.Column<bool>(type: "boolean", nullable: false),
                    infract_on_invite = table.Column<bool>(type: "boolean", nullable: false),
                    delete_invite_messages = table.Column<bool>(type: "boolean", nullable: false),
                    match_aggressively = table.Column<bool>(type: "boolean", nullable: false),
                    progressive_infractions = table.Column<bool>(type: "boolean", nullable: false),
                    AutoDehoist = table.Column<bool>(type: "boolean", nullable: false),
                    detect_phishing = table.Column<bool>(type: "boolean", nullable: false),
                    delete_detected_phishing = table.Column<bool>(type: "boolean", nullable: false),
                    scan_invite_origin = table.Column<bool>(type: "boolean", nullable: false),
                    UseWebhookLogging = table.Column<bool>(type: "boolean", nullable: false),
                    WebhookLoggingId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LoggingWebhookUrl = table.Column<string>(type: "text", nullable: true),
                    LoggingConfigId = table.Column<int>(type: "integer", nullable: false),
                    NamedInfractionSteps = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_moderation_config", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guild_moderation_config_guild_logging_configs_LoggingConfig~",
                        column: x => x.LoggingConfigId,
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

            migrationBuilder.CreateTable(
                name: "infraction_exemptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    exempt_from = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    target_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    GuildModConfigEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_infraction_exemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_infraction_exemptions_guild_moderation_config_GuildModConfi~",
                        column: x => x.GuildModConfigEntityId,
                        principalTable: "guild_moderation_config",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "infraction_steps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    config_id = table.Column<int>(type: "integer", nullable: false),
                    infraction_count = table.Column<int>(type: "integer", nullable: false),
                    infraction_type = table.Column<int>(type: "integer", nullable: false),
                    infraction_duration = table.Column<long>(type: "bigint", nullable: false),
                    GuildModConfigEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_infraction_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_infraction_steps_guild_moderation_config_GuildModConfigEnti~",
                        column: x => x.GuildModConfigEntityId,
                        principalTable: "guild_moderation_config",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "invites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    invite_guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    invite_code = table.Column<string>(type: "text", nullable: false),
                    GuildModConfigEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invites_guild_moderation_config_GuildModConfigEntityId",
                        column: x => x.GuildModConfigEntityId,
                        principalTable: "guild_moderation_config",
                        principalColumn: "Id");
                });

            migrationBuilder.Sql("ALTER TABLE \"infraction_exemptions\" ALTER COLUMN \"exempt_from\" TYPE integer USING (\"exempt_from\"::integer)");
            
            migrationBuilder.CreateIndex(
                name: "IX_disbaled_commands_GuildConfigEntityId",
                table: "disbaled_commands",
                column: "GuildConfigEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_disbaled_commands_GuildID_CommandName",
                table: "disbaled_commands",
                columns: new[] { "GuildID", "CommandName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_guild_configs_guild_id",
                table: "guild_configs",
                column: "guild_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_guild_greetings_GuildConfigEntityId",
                table: "guild_greetings",
                column: "GuildConfigEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_guild_logging_configs_InfractionsId",
                table: "guild_logging_configs",
                column: "InfractionsId");

            migrationBuilder.CreateIndex(
                name: "IX_guild_logging_configs_MemberJoinsId",
                table: "guild_logging_configs",
                column: "MemberJoinsId");

            migrationBuilder.CreateIndex(
                name: "IX_guild_logging_configs_MemberLeavesId",
                table: "guild_logging_configs",
                column: "MemberLeavesId");

            migrationBuilder.CreateIndex(
                name: "IX_guild_logging_configs_MessageDeletesId",
                table: "guild_logging_configs",
                column: "MessageDeletesId");

            migrationBuilder.CreateIndex(
                name: "IX_guild_logging_configs_MessageEditsId",
                table: "guild_logging_configs",
                column: "MessageEditsId");

            migrationBuilder.CreateIndex(
                name: "IX_guild_moderation_config_guild_id",
                table: "guild_moderation_config",
                column: "guild_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_guild_moderation_config_LoggingConfigId",
                table: "guild_moderation_config",
                column: "LoggingConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_guild_tags_GuildEntityID",
                table: "guild_tags",
                column: "GuildEntityID");

            migrationBuilder.CreateIndex(
                name: "IX_guild_tags_parent_id",
                table: "guild_tags",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_infraction_exemptions_GuildModConfigEntityId",
                table: "infraction_exemptions",
                column: "GuildModConfigEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_infraction_steps_GuildModConfigEntityId",
                table: "infraction_steps",
                column: "GuildModConfigEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_infractions_guild_id",
                table: "infractions",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_infractions_target_id_guild_id",
                table: "infractions",
                columns: new[] { "target_id", "guild_id" });

            migrationBuilder.CreateIndex(
                name: "IX_invites_GuildModConfigEntityId",
                table: "invites",
                column: "GuildModConfigEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_user_histories_user_id_guild_id",
                table: "user_histories",
                columns: new[] { "user_id", "guild_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_guild_id",
                table: "users",
                column: "guild_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "command_invocations");

            migrationBuilder.DropTable(
                name: "disbaled_commands");

            migrationBuilder.DropTable(
                name: "guild_greetings");

            migrationBuilder.DropTable(
                name: "guild_tags");

            migrationBuilder.DropTable(
                name: "infraction_exemptions");

            migrationBuilder.DropTable(
                name: "infraction_steps");

            migrationBuilder.DropTable(
                name: "infractions");

            migrationBuilder.DropTable(
                name: "invites");

            migrationBuilder.DropTable(
                name: "reminders");

            migrationBuilder.DropTable(
                name: "user_histories");

            migrationBuilder.DropTable(
                name: "guild_configs");

            migrationBuilder.DropTable(
                name: "guild_moderation_config");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "guild_logging_configs");

            migrationBuilder.DropTable(
                name: "guilds");

            migrationBuilder.DropTable(
                name: "logging_channels");
        }
    }
}
