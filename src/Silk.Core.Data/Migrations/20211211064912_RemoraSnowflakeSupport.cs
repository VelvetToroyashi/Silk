using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class RemoraSnowflakeSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DisabledCommandEntity_GuildConfigs_GuildConfigEntityId",
                table: "DisabledCommandEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_DisabledCommandEntity_Guilds_GuildId",
                table: "DisabledCommandEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_ExemptionEntity_GuildModConfigs_GuildModConfigEntityId",
                table: "ExemptionEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildConfigs_Guilds_GuildId",
                table: "GuildConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildGreetingEntity_GuildConfigs_GuildConfigEntityId",
                table: "GuildGreetingEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_InfractionsId",
                table: "GuildLoggingConfigEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_MemberJoinsId",
                table: "GuildLoggingConfigEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_MemberLeavesId",
                table: "GuildLoggingConfigEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_MessageDelete~",
                table: "GuildLoggingConfigEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_MessageEditsId",
                table: "GuildLoggingConfigEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildModConfigs_GuildLoggingConfigEntity_LoggingConfigId",
                table: "GuildModConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildModConfigs_Guilds_GuildId",
                table: "GuildModConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_Guilds_GuildId",
                table: "Infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_Users_UserId_GuildId",
                table: "Infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_InfractionStepEntity_GuildModConfigs_ConfigId",
                table: "InfractionStepEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_InviteEntity_GuildModConfigs_GuildModConfigEntityId",
                table: "InviteEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Tags_OriginalTagId",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHistoryEntity_Users_UserId_GuildId",
                table: "UserHistoryEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_InfractionStepEntity_ConfigId",
                table: "InfractionStepEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildModConfigs",
                table: "GuildModConfigs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildLoggingConfigEntity",
                table: "GuildLoggingConfigEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildConfigs",
                table: "GuildConfigs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExemptionEntity",
                table: "ExemptionEntity");

            migrationBuilder.DropColumn(
                name: "DatabaseId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InitialJoinDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "Expiration",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "WasReply",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "Expiration",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "InfractionTime",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Infractions");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "GuildModConfigs",
                newName: "guild_moderation_config");

            migrationBuilder.RenameTable(
                name: "GuildLoggingConfigEntity",
                newName: "guild_logging_configs");

            migrationBuilder.RenameTable(
                name: "GuildConfigs",
                newName: "guild_configs");

            migrationBuilder.RenameTable(
                name: "ExemptionEntity",
                newName: "exemption_entity");

            migrationBuilder.RenameColumn(
                name: "Flags",
                table: "users",
                newName: "flags");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "users",
                newName: "guild_id");

            migrationBuilder.RenameIndex(
                name: "IX_Users_GuildId",
                table: "users",
                newName: "IX_users_guild_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserHistoryEntity",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "LeaveDates",
                table: "UserHistoryEntity",
                newName: "leave_dates");

            migrationBuilder.RenameColumn(
                name: "JoinDates",
                table: "UserHistoryEntity",
                newName: "join_dates");

            migrationBuilder.RenameColumn(
                name: "JoinDate",
                table: "UserHistoryEntity",
                newName: "initial_join_date");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "UserHistoryEntity",
                newName: "guild_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserHistoryEntity_UserId_GuildId",
                table: "UserHistoryEntity",
                newName: "IX_UserHistoryEntity_user_id_guild_id");

            migrationBuilder.RenameColumn(
                name: "Uses",
                table: "Tags",
                newName: "uses");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tags",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Tags",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Tags",
                newName: "owner_id");

            migrationBuilder.RenameColumn(
                name: "OriginalTagId",
                table: "Tags",
                newName: "parent_id");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "Tags",
                newName: "guild_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Tags",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_OriginalTagId",
                table: "Tags",
                newName: "IX_Tags_parent_id");

            migrationBuilder.RenameColumn(
                name: "ReplyMessageContent",
                table: "Reminders",
                newName: "reply_content");

            migrationBuilder.RenameColumn(
                name: "ReplyId",
                table: "Reminders",
                newName: "reply_message_id");

            migrationBuilder.RenameColumn(
                name: "ReplyAuthorId",
                table: "Reminders",
                newName: "reply_author_id");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Reminders",
                newName: "owner_id");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "Reminders",
                newName: "message_id");

            migrationBuilder.RenameColumn(
                name: "MessageContent",
                table: "Reminders",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "Reminders",
                newName: "guild_id");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "Reminders",
                newName: "channel_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "InfractionStepEntity",
                newName: "infraction_type");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "InfractionStepEntity",
                newName: "infraction_duration");

            migrationBuilder.RenameColumn(
                name: "ConfigId",
                table: "InfractionStepEntity",
                newName: "config_id");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "Infractions",
                newName: "reason");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "Infractions",
                newName: "guild_id");

            migrationBuilder.RenameColumn(
                name: "CaseNumber",
                table: "Infractions",
                newName: "case_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Infractions",
                newName: "target_id");

            migrationBuilder.RenameColumn(
                name: "InfractionType",
                table: "Infractions",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "HeldAgainstUser",
                table: "Infractions",
                newName: "processed");

            migrationBuilder.RenameColumn(
                name: "Handled",
                table: "Infractions",
                newName: "escalated");

            migrationBuilder.RenameColumn(
                name: "EscalatedFromStrike",
                table: "Infractions",
                newName: "active");

            migrationBuilder.RenameColumn(
                name: "Enforcer",
                table: "Infractions",
                newName: "enforcer_id");

            migrationBuilder.RenameIndex(
                name: "IX_Infractions_UserId_GuildId",
                table: "Infractions",
                newName: "IX_Infractions_target_id_guild_id");

            migrationBuilder.RenameIndex(
                name: "IX_Infractions_GuildId",
                table: "Infractions",
                newName: "IX_Infractions_guild_id");

            migrationBuilder.RenameColumn(
                name: "Prefix",
                table: "Guilds",
                newName: "prefix");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "GuildGreetingEntity",
                newName: "GuildID");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "GuildGreetingEntity",
                newName: "ChannelID");

            migrationBuilder.RenameColumn(
                name: "MetadataSnowflake",
                table: "GuildGreetingEntity",
                newName: "MetadataID");

            migrationBuilder.RenameColumn(
                name: "InvocationTime",
                table: "CommandInvocations",
                newName: "used_at");

            migrationBuilder.RenameColumn(
                name: "CommandName",
                table: "CommandInvocations",
                newName: "command_name");

            migrationBuilder.RenameColumn(
                name: "UseAggressiveRegex",
                table: "guild_moderation_config",
                newName: "match_aggressively");

            migrationBuilder.RenameColumn(
                name: "MuteRoleId",
                table: "guild_moderation_config",
                newName: "mute_role");

            migrationBuilder.RenameColumn(
                name: "MaxUserMentions",
                table: "guild_moderation_config",
                newName: "max_user_mentions");

            migrationBuilder.RenameColumn(
                name: "MaxRoleMentions",
                table: "guild_moderation_config",
                newName: "max_role_mentions");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "guild_moderation_config",
                newName: "guild_id");

            migrationBuilder.RenameColumn(
                name: "DetectPhishingLinks",
                table: "guild_moderation_config",
                newName: "detect_phishing");

            migrationBuilder.RenameColumn(
                name: "DeletePhishingLinks",
                table: "guild_moderation_config",
                newName: "delete_detected_phishing");

            migrationBuilder.RenameColumn(
                name: "DeleteMessageOnMatchedInvite",
                table: "guild_moderation_config",
                newName: "delete_invite_messages");

            migrationBuilder.RenameColumn(
                name: "WarnOnMatchedInvite",
                table: "guild_moderation_config",
                newName: "scan_invite_origin");

            migrationBuilder.RenameColumn(
                name: "ScanInvites",
                table: "guild_moderation_config",
                newName: "invite_whitelist_enabled");

            migrationBuilder.RenameColumn(
                name: "BlacklistInvites",
                table: "guild_moderation_config",
                newName: "infract_on_invite");

            migrationBuilder.RenameColumn(
                name: "AutoEscalateInfractions",
                table: "guild_moderation_config",
                newName: "ProgressiveStriking");

            migrationBuilder.RenameIndex(
                name: "IX_GuildModConfigs_LoggingConfigId",
                table: "guild_moderation_config",
                newName: "IX_guild_moderation_config_LoggingConfigId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildModConfigs_GuildId",
                table: "guild_moderation_config",
                newName: "IX_guild_moderation_config_guild_id");

            migrationBuilder.RenameIndex(
                name: "IX_GuildLoggingConfigEntity_MessageEditsId",
                table: "guild_logging_configs",
                newName: "IX_guild_logging_configs_MessageEditsId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildLoggingConfigEntity_MessageDeletesId",
                table: "guild_logging_configs",
                newName: "IX_guild_logging_configs_MessageDeletesId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildLoggingConfigEntity_MemberLeavesId",
                table: "guild_logging_configs",
                newName: "IX_guild_logging_configs_MemberLeavesId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildLoggingConfigEntity_MemberJoinsId",
                table: "guild_logging_configs",
                newName: "IX_guild_logging_configs_MemberJoinsId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildLoggingConfigEntity_InfractionsId",
                table: "guild_logging_configs",
                newName: "IX_guild_logging_configs_InfractionsId");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "guild_configs",
                newName: "guild_id");

            migrationBuilder.RenameIndex(
                name: "IX_GuildConfigs_GuildId",
                table: "guild_configs",
                newName: "IX_guild_configs_guild_id");

            migrationBuilder.RenameIndex(
                name: "IX_ExemptionEntity_GuildModConfigEntityId",
                table: "exemption_entity",
                newName: "IX_exemption_entity_GuildModConfigEntityId");

            migrationBuilder.AlterColumn<List<DateTimeOffset>>(
                name: "leave_dates",
                table: "UserHistoryEntity",
                type: "timestamp with time zone[]",
                nullable: false,
                oldClrType: typeof(List<DateTime>),
                oldType: "timestamp without time zone[]");

            migrationBuilder.AlterColumn<List<DateTimeOffset>>(
                name: "join_dates",
                table: "UserHistoryEntity",
                type: "timestamp with time zone[]",
                nullable: false,
                oldClrType: typeof(List<DateTime>),
                oldType: "timestamp without time zone[]");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "initial_join_date",
                table: "UserHistoryEntity",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "Tags",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<ulong>(
                name: "message_id",
                table: "Reminders",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "Reminders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expires_at",
                table: "Reminders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "GuildModConfigEntityId",
                table: "InfractionStepEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "infraction_count",
                table: "InfractionStepEntity",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "Infractions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expires_at",
                table: "Infractions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId1",
                table: "DisabledCommandEntity",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.Sql("ALTER TABLE exemption_entity ALTER COLUMN exempt_from SET DATA TYPE integer USING exempt_from::integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                columns: new[] { "id", "guild_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_guild_moderation_config",
                table: "guild_moderation_config",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_guild_logging_configs",
                table: "guild_logging_configs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_guild_configs",
                table: "guild_configs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_exemption_entity",
                table: "exemption_entity",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_InfractionStepEntity_GuildModConfigEntityId",
                table: "InfractionStepEntity",
                column: "GuildModConfigEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DisabledCommandEntity_GuildId1",
                table: "DisabledCommandEntity",
                column: "GuildId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DisabledCommandEntity_guild_configs_GuildConfigEntityId",
                table: "DisabledCommandEntity",
                column: "GuildConfigEntityId",
                principalTable: "guild_configs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DisabledCommandEntity_Guilds_GuildId1",
                table: "DisabledCommandEntity",
                column: "GuildId1",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_exemption_entity_guild_moderation_config_GuildModConfigEnti~",
                table: "exemption_entity",
                column: "GuildModConfigEntityId",
                principalTable: "guild_moderation_config",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_configs_Guilds_guild_id",
                table: "guild_configs",
                column: "guild_id",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_logging_configs_LoggingChannelEntity_InfractionsId",
                table: "guild_logging_configs",
                column: "InfractionsId",
                principalTable: "LoggingChannelEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_logging_configs_LoggingChannelEntity_MemberJoinsId",
                table: "guild_logging_configs",
                column: "MemberJoinsId",
                principalTable: "LoggingChannelEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_logging_configs_LoggingChannelEntity_MemberLeavesId",
                table: "guild_logging_configs",
                column: "MemberLeavesId",
                principalTable: "LoggingChannelEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_logging_configs_LoggingChannelEntity_MessageDeletesId",
                table: "guild_logging_configs",
                column: "MessageDeletesId",
                principalTable: "LoggingChannelEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_logging_configs_LoggingChannelEntity_MessageEditsId",
                table: "guild_logging_configs",
                column: "MessageEditsId",
                principalTable: "LoggingChannelEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_moderation_config_guild_logging_configs_LoggingConfig~",
                table: "guild_moderation_config",
                column: "LoggingConfigId",
                principalTable: "guild_logging_configs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_moderation_config_Guilds_guild_id",
                table: "guild_moderation_config",
                column: "guild_id",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildGreetingEntity_guild_configs_GuildConfigEntityId",
                table: "GuildGreetingEntity",
                column: "GuildConfigEntityId",
                principalTable: "guild_configs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_Guilds_guild_id",
                table: "Infractions",
                column: "guild_id",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_users_target_id_guild_id",
                table: "Infractions",
                columns: new[] { "target_id", "guild_id" },
                principalTable: "users",
                principalColumns: new[] { "id", "guild_id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InfractionStepEntity_guild_moderation_config_GuildModConfig~",
                table: "InfractionStepEntity",
                column: "GuildModConfigEntityId",
                principalTable: "guild_moderation_config",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InviteEntity_guild_moderation_config_GuildModConfigEntityId",
                table: "InviteEntity",
                column: "GuildModConfigEntityId",
                principalTable: "guild_moderation_config",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Tags_parent_id",
                table: "Tags",
                column: "parent_id",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHistoryEntity_users_user_id_guild_id",
                table: "UserHistoryEntity",
                columns: new[] { "user_id", "guild_id" },
                principalTable: "users",
                principalColumns: new[] { "id", "guild_id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_Guilds_guild_id",
                table: "users",
                column: "guild_id",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DisabledCommandEntity_guild_configs_GuildConfigEntityId",
                table: "DisabledCommandEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_DisabledCommandEntity_Guilds_GuildId1",
                table: "DisabledCommandEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_exemption_entity_guild_moderation_config_GuildModConfigEnti~",
                table: "exemption_entity");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_configs_Guilds_guild_id",
                table: "guild_configs");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_logging_configs_LoggingChannelEntity_InfractionsId",
                table: "guild_logging_configs");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_logging_configs_LoggingChannelEntity_MemberJoinsId",
                table: "guild_logging_configs");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_logging_configs_LoggingChannelEntity_MemberLeavesId",
                table: "guild_logging_configs");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_logging_configs_LoggingChannelEntity_MessageDeletesId",
                table: "guild_logging_configs");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_logging_configs_LoggingChannelEntity_MessageEditsId",
                table: "guild_logging_configs");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_moderation_config_guild_logging_configs_LoggingConfig~",
                table: "guild_moderation_config");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_moderation_config_Guilds_guild_id",
                table: "guild_moderation_config");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildGreetingEntity_guild_configs_GuildConfigEntityId",
                table: "GuildGreetingEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_Guilds_guild_id",
                table: "Infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_users_target_id_guild_id",
                table: "Infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_InfractionStepEntity_guild_moderation_config_GuildModConfig~",
                table: "InfractionStepEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_InviteEntity_guild_moderation_config_GuildModConfigEntityId",
                table: "InviteEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Tags_parent_id",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHistoryEntity_users_user_id_guild_id",
                table: "UserHistoryEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_users_Guilds_guild_id",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_InfractionStepEntity_GuildModConfigEntityId",
                table: "InfractionStepEntity");

            migrationBuilder.DropIndex(
                name: "IX_DisabledCommandEntity_GuildId1",
                table: "DisabledCommandEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guild_moderation_config",
                table: "guild_moderation_config");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guild_logging_configs",
                table: "guild_logging_configs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guild_configs",
                table: "guild_configs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_exemption_entity",
                table: "exemption_entity");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "expires_at",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "GuildModConfigEntityId",
                table: "InfractionStepEntity");

            migrationBuilder.DropColumn(
                name: "infraction_count",
                table: "InfractionStepEntity");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "expires_at",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "GuildId1",
                table: "DisabledCommandEntity");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "guild_moderation_config",
                newName: "GuildModConfigs");

            migrationBuilder.RenameTable(
                name: "guild_logging_configs",
                newName: "GuildLoggingConfigEntity");

            migrationBuilder.RenameTable(
                name: "guild_configs",
                newName: "GuildConfigs");

            migrationBuilder.RenameTable(
                name: "exemption_entity",
                newName: "ExemptionEntity");

            migrationBuilder.RenameColumn(
                name: "flags",
                table: "Users",
                newName: "Flags");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "Users",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_users_guild_id",
                table: "Users",
                newName: "IX_Users_GuildId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserHistoryEntity",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "leave_dates",
                table: "UserHistoryEntity",
                newName: "LeaveDates");

            migrationBuilder.RenameColumn(
                name: "join_dates",
                table: "UserHistoryEntity",
                newName: "JoinDates");

            migrationBuilder.RenameColumn(
                name: "initial_join_date",
                table: "UserHistoryEntity",
                newName: "JoinDate");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "UserHistoryEntity",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_UserHistoryEntity_user_id_guild_id",
                table: "UserHistoryEntity",
                newName: "IX_UserHistoryEntity_UserId_GuildId");

            migrationBuilder.RenameColumn(
                name: "uses",
                table: "Tags",
                newName: "Uses");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Tags",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "Tags",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "parent_id",
                table: "Tags",
                newName: "OriginalTagId");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                table: "Tags",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "Tags",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Tags",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_parent_id",
                table: "Tags",
                newName: "IX_Tags_OriginalTagId");

            migrationBuilder.RenameColumn(
                name: "reply_message_id",
                table: "Reminders",
                newName: "ReplyId");

            migrationBuilder.RenameColumn(
                name: "reply_content",
                table: "Reminders",
                newName: "ReplyMessageContent");

            migrationBuilder.RenameColumn(
                name: "reply_author_id",
                table: "Reminders",
                newName: "ReplyAuthorId");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                table: "Reminders",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "message_id",
                table: "Reminders",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "Reminders",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "Reminders",
                newName: "MessageContent");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "Reminders",
                newName: "ChannelId");

            migrationBuilder.RenameColumn(
                name: "infraction_type",
                table: "InfractionStepEntity",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "infraction_duration",
                table: "InfractionStepEntity",
                newName: "Duration");

            migrationBuilder.RenameColumn(
                name: "config_id",
                table: "InfractionStepEntity",
                newName: "ConfigId");

            migrationBuilder.RenameColumn(
                name: "reason",
                table: "Infractions",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "Infractions",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "case_id",
                table: "Infractions",
                newName: "CaseNumber");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "Infractions",
                newName: "InfractionType");

            migrationBuilder.RenameColumn(
                name: "target_id",
                table: "Infractions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "processed",
                table: "Infractions",
                newName: "HeldAgainstUser");

            migrationBuilder.RenameColumn(
                name: "escalated",
                table: "Infractions",
                newName: "Handled");

            migrationBuilder.RenameColumn(
                name: "enforcer_id",
                table: "Infractions",
                newName: "Enforcer");

            migrationBuilder.RenameColumn(
                name: "active",
                table: "Infractions",
                newName: "EscalatedFromStrike");

            migrationBuilder.RenameIndex(
                name: "IX_Infractions_target_id_guild_id",
                table: "Infractions",
                newName: "IX_Infractions_UserId_GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_Infractions_guild_id",
                table: "Infractions",
                newName: "IX_Infractions_GuildId");

            migrationBuilder.RenameColumn(
                name: "prefix",
                table: "Guilds",
                newName: "Prefix");

            migrationBuilder.RenameColumn(
                name: "GuildID",
                table: "GuildGreetingEntity",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "ChannelID",
                table: "GuildGreetingEntity",
                newName: "ChannelId");

            migrationBuilder.RenameColumn(
                name: "MetadataID",
                table: "GuildGreetingEntity",
                newName: "MetadataSnowflake");

            migrationBuilder.RenameColumn(
                name: "used_at",
                table: "CommandInvocations",
                newName: "InvocationTime");

            migrationBuilder.RenameColumn(
                name: "command_name",
                table: "CommandInvocations",
                newName: "CommandName");

            migrationBuilder.RenameColumn(
                name: "mute_role",
                table: "GuildModConfigs",
                newName: "MuteRoleId");

            migrationBuilder.RenameColumn(
                name: "max_user_mentions",
                table: "GuildModConfigs",
                newName: "MaxUserMentions");

            migrationBuilder.RenameColumn(
                name: "max_role_mentions",
                table: "GuildModConfigs",
                newName: "MaxRoleMentions");

            migrationBuilder.RenameColumn(
                name: "match_aggressively",
                table: "GuildModConfigs",
                newName: "UseAggressiveRegex");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "GuildModConfigs",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "detect_phishing",
                table: "GuildModConfigs",
                newName: "DetectPhishingLinks");

            migrationBuilder.RenameColumn(
                name: "delete_invite_messages",
                table: "GuildModConfigs",
                newName: "DeleteMessageOnMatchedInvite");

            migrationBuilder.RenameColumn(
                name: "delete_detected_phishing",
                table: "GuildModConfigs",
                newName: "DeletePhishingLinks");

            migrationBuilder.RenameColumn(
                name: "scan_invite_origin",
                table: "GuildModConfigs",
                newName: "WarnOnMatchedInvite");

            migrationBuilder.RenameColumn(
                name: "invite_whitelist_enabled",
                table: "GuildModConfigs",
                newName: "ScanInvites");

            migrationBuilder.RenameColumn(
                name: "infract_on_invite",
                table: "GuildModConfigs",
                newName: "BlacklistInvites");

            migrationBuilder.RenameColumn(
                name: "ProgressiveStriking",
                table: "GuildModConfigs",
                newName: "AutoEscalateInfractions");

            migrationBuilder.RenameIndex(
                name: "IX_guild_moderation_config_LoggingConfigId",
                table: "GuildModConfigs",
                newName: "IX_GuildModConfigs_LoggingConfigId");

            migrationBuilder.RenameIndex(
                name: "IX_guild_moderation_config_guild_id",
                table: "GuildModConfigs",
                newName: "IX_GuildModConfigs_GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_guild_logging_configs_MessageEditsId",
                table: "GuildLoggingConfigEntity",
                newName: "IX_GuildLoggingConfigEntity_MessageEditsId");

            migrationBuilder.RenameIndex(
                name: "IX_guild_logging_configs_MessageDeletesId",
                table: "GuildLoggingConfigEntity",
                newName: "IX_GuildLoggingConfigEntity_MessageDeletesId");

            migrationBuilder.RenameIndex(
                name: "IX_guild_logging_configs_MemberLeavesId",
                table: "GuildLoggingConfigEntity",
                newName: "IX_GuildLoggingConfigEntity_MemberLeavesId");

            migrationBuilder.RenameIndex(
                name: "IX_guild_logging_configs_MemberJoinsId",
                table: "GuildLoggingConfigEntity",
                newName: "IX_GuildLoggingConfigEntity_MemberJoinsId");

            migrationBuilder.RenameIndex(
                name: "IX_guild_logging_configs_InfractionsId",
                table: "GuildLoggingConfigEntity",
                newName: "IX_GuildLoggingConfigEntity_InfractionsId");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "GuildConfigs",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_guild_configs_guild_id",
                table: "GuildConfigs",
                newName: "IX_GuildConfigs_GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_exemption_entity_GuildModConfigEntityId",
                table: "ExemptionEntity",
                newName: "IX_ExemptionEntity_GuildModConfigEntityId");

            migrationBuilder.AddColumn<long>(
                name: "DatabaseId",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "InitialJoinDate",
                table: "Users",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<List<DateTime>>(
                name: "LeaveDates",
                table: "UserHistoryEntity",
                type: "timestamp without time zone[]",
                nullable: false,
                oldClrType: typeof(List<DateTimeOffset>),
                oldType: "timestamp with time zone[]");

            migrationBuilder.AlterColumn<List<DateTime>>(
                name: "JoinDates",
                table: "UserHistoryEntity",
                type: "timestamp without time zone[]",
                nullable: false,
                oldClrType: typeof(List<DateTimeOffset>),
                oldType: "timestamp with time zone[]");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinDate",
                table: "UserHistoryEntity",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Tags",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<decimal>(
                name: "MessageId",
                table: "Reminders",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Reminders",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Expiration",
                table: "Reminders",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "WasReply",
                table: "Reminders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Expiration",
                table: "Infractions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InfractionTime",
                table: "Infractions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Infractions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "exempt_from",
                table: "ExemptionEntity",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                columns: new[] { "Id", "GuildId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildModConfigs",
                table: "GuildModConfigs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildLoggingConfigEntity",
                table: "GuildLoggingConfigEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildConfigs",
                table: "GuildConfigs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExemptionEntity",
                table: "ExemptionEntity",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_InfractionStepEntity_ConfigId",
                table: "InfractionStepEntity",
                column: "ConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_DisabledCommandEntity_GuildConfigs_GuildConfigEntityId",
                table: "DisabledCommandEntity",
                column: "GuildConfigEntityId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DisabledCommandEntity_Guilds_GuildId",
                table: "DisabledCommandEntity",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExemptionEntity_GuildModConfigs_GuildModConfigEntityId",
                table: "ExemptionEntity",
                column: "GuildModConfigEntityId",
                principalTable: "GuildModConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildConfigs_Guilds_GuildId",
                table: "GuildConfigs",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildGreetingEntity_GuildConfigs_GuildConfigEntityId",
                table: "GuildGreetingEntity",
                column: "GuildConfigEntityId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_InfractionsId",
                table: "GuildLoggingConfigEntity",
                column: "InfractionsId",
                principalTable: "LoggingChannelEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_MemberJoinsId",
                table: "GuildLoggingConfigEntity",
                column: "MemberJoinsId",
                principalTable: "LoggingChannelEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_MemberLeavesId",
                table: "GuildLoggingConfigEntity",
                column: "MemberLeavesId",
                principalTable: "LoggingChannelEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_MessageDelete~",
                table: "GuildLoggingConfigEntity",
                column: "MessageDeletesId",
                principalTable: "LoggingChannelEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_MessageEditsId",
                table: "GuildLoggingConfigEntity",
                column: "MessageEditsId",
                principalTable: "LoggingChannelEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildModConfigs_GuildLoggingConfigEntity_LoggingConfigId",
                table: "GuildModConfigs",
                column: "LoggingConfigId",
                principalTable: "GuildLoggingConfigEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildModConfigs_Guilds_GuildId",
                table: "GuildModConfigs",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_Guilds_GuildId",
                table: "Infractions",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_Users_UserId_GuildId",
                table: "Infractions",
                columns: new[] { "UserId", "GuildId" },
                principalTable: "Users",
                principalColumns: new[] { "Id", "GuildId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InfractionStepEntity_GuildModConfigs_ConfigId",
                table: "InfractionStepEntity",
                column: "ConfigId",
                principalTable: "GuildModConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InviteEntity_GuildModConfigs_GuildModConfigEntityId",
                table: "InviteEntity",
                column: "GuildModConfigEntityId",
                principalTable: "GuildModConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Tags_OriginalTagId",
                table: "Tags",
                column: "OriginalTagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHistoryEntity_Users_UserId_GuildId",
                table: "UserHistoryEntity",
                columns: new[] { "UserId", "GuildId" },
                principalTable: "Users",
                principalColumns: new[] { "Id", "GuildId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
