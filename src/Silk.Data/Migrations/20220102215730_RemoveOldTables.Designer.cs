﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Silk.Data;

#nullable disable

namespace Silk.Data.Migrations
{
    [DbContext(typeof(GuildContext))]
    [Migration("20220102215730_RemoveOldTables")]
    partial class RemoveOldTables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Silk.Data.Entities.CommandInvocationEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("CommandName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("command_name");

                    b.Property<DateTime>("InvocationTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("used_at");

                    b.HasKey("Id");

                    b.ToTable("command_invocations");
                });

            modelBuilder.Entity("Silk.Data.Entities.ExemptionEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Exemption")
                        .HasColumnType("integer")
                        .HasColumnName("exempt_from");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<int?>("GuildModConfigEntityId")
                        .HasColumnType("integer");

                    b.Property<ulong>("TargetID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("target_id");

                    b.Property<int>("TargetType")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.HasIndex("GuildModConfigEntityId");

                    b.ToTable("infraction_exemptions");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildConfigEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.HasKey("Id");

                    b.HasIndex("GuildID")
                        .IsUnique();

                    b.ToTable("guild_configs");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildEntity", b =>
                {
                    b.Property<ulong>("ID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("Id");

                    b.Property<string>("Prefix")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("character varying(5)")
                        .HasColumnName("prefix");

                    b.HasKey("ID");

                    b.ToTable("guilds");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildGreetingEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<ulong>("ChannelID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("GuildConfigEntityId")
                        .HasColumnType("integer");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<ulong?>("MetadataID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Option")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GuildConfigEntityId");

                    b.ToTable("guild_greetings");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildLoggingConfigEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<ulong?>("FallbackChannelID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("fallback_logging_channel");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<int?>("InfractionsId")
                        .HasColumnType("integer");

                    b.Property<bool>("LogInfractions")
                        .HasColumnType("boolean")
                        .HasColumnName("log_infractions");

                    b.Property<bool>("LogMemberJoins")
                        .HasColumnType("boolean")
                        .HasColumnName("log_member_joins");

                    b.Property<bool>("LogMemberLeaves")
                        .HasColumnType("boolean")
                        .HasColumnName("log_member_leaves");

                    b.Property<bool>("LogMessageDeletes")
                        .HasColumnType("boolean")
                        .HasColumnName("log_message_deletes");

                    b.Property<bool>("LogMessageEdits")
                        .HasColumnType("boolean")
                        .HasColumnName("log_message_edits");

                    b.Property<int?>("MemberJoinsId")
                        .HasColumnType("integer");

                    b.Property<int?>("MemberLeavesId")
                        .HasColumnType("integer");

                    b.Property<int?>("MessageDeletesId")
                        .HasColumnType("integer");

                    b.Property<int?>("MessageEditsId")
                        .HasColumnType("integer");

                    b.Property<bool>("UseWebhookLogging")
                        .HasColumnType("boolean")
                        .HasColumnName("use_webhook_logging");

                    b.HasKey("Id");

                    b.HasIndex("InfractionsId");

                    b.HasIndex("MemberJoinsId");

                    b.HasIndex("MemberLeavesId");

                    b.HasIndex("MessageDeletesId");

                    b.HasIndex("MessageEditsId");

                    b.ToTable("guild_logging_configs");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildModConfigEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("DeleteMessageOnMatchedInvite")
                        .HasColumnType("boolean")
                        .HasColumnName("delete_invite_messages");

                    b.Property<bool>("DeletePhishingLinks")
                        .HasColumnType("boolean")
                        .HasColumnName("delete_detected_phishing");

                    b.Property<bool>("DetectPhishingLinks")
                        .HasColumnType("boolean")
                        .HasColumnName("detect_phishing");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<bool>("InfractOnMatchedInvite")
                        .HasColumnType("boolean")
                        .HasColumnName("infract_on_invite");

                    b.Property<int>("LoggingConfigId")
                        .HasColumnType("integer");

                    b.Property<int>("MaxRoleMentions")
                        .HasColumnType("integer")
                        .HasColumnName("max_role_mentions");

                    b.Property<int>("MaxUserMentions")
                        .HasColumnType("integer")
                        .HasColumnName("max_user_mentions");

                    b.Property<ulong>("MuteRoleID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("mute_role");

                    b.Property<string>("NamedInfractionSteps")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("ProgressiveStriking")
                        .HasColumnType("boolean")
                        .HasColumnName("progressive_infractions");

                    b.Property<bool>("ScanInviteOrigin")
                        .HasColumnType("boolean")
                        .HasColumnName("scan_invite_origin");

                    b.Property<bool>("UseAggressiveRegex")
                        .HasColumnType("boolean")
                        .HasColumnName("match_aggressively");

                    b.Property<bool>("WhitelistInvites")
                        .HasColumnType("boolean")
                        .HasColumnName("invite_whitelist_enabled");

                    b.HasKey("Id");

                    b.HasIndex("GuildID")
                        .IsUnique();

                    b.HasIndex("LoggingConfigId");

                    b.ToTable("guild_moderation_config");
                });

            modelBuilder.Entity("Silk.Data.Entities.InfractionEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("AppliesToTarget")
                        .HasColumnType("boolean")
                        .HasColumnName("active");

                    b.Property<int>("CaseNumber")
                        .HasColumnType("integer")
                        .HasColumnName("case_id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<ulong>("EnforcerID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("enforcer_id");

                    b.Property<bool>("Escalated")
                        .HasColumnType("boolean")
                        .HasColumnName("escalated");

                    b.Property<DateTimeOffset?>("ExpiresAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expires_at");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<bool>("Processed")
                        .HasColumnType("boolean")
                        .HasColumnName("processed");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("reason");

                    b.Property<ulong>("TargetID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("target_id");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.Property<bool>("UserNotified")
                        .HasColumnType("boolean")
                        .HasColumnName("user_notified");

                    b.HasKey("Id");

                    b.HasIndex("GuildID");

                    b.HasIndex("TargetID", "GuildID");

                    b.ToTable("infractions");
                });

            modelBuilder.Entity("Silk.Data.Entities.InfractionStepEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ConfigId")
                        .HasColumnType("integer")
                        .HasColumnName("config_id");

                    b.Property<long>("Duration")
                        .HasColumnType("bigint")
                        .HasColumnName("infraction_duration");

                    b.Property<int?>("GuildModConfigEntityId")
                        .HasColumnType("integer");

                    b.Property<int>("Infractions")
                        .HasColumnType("integer")
                        .HasColumnName("infraction_count");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("infraction_type");

                    b.HasKey("Id");

                    b.HasIndex("GuildModConfigEntityId");

                    b.ToTable("infraction_steps");
                });

            modelBuilder.Entity("Silk.Data.Entities.InviteEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<ulong>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<int?>("GuildModConfigEntityId")
                        .HasColumnType("integer");

                    b.Property<ulong>("InviteGuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("invite_guild_id");

                    b.Property<string>("VanityURL")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("invite_code");

                    b.HasKey("Id");

                    b.HasIndex("GuildModConfigEntityId");

                    b.ToTable("invites");
                });

            modelBuilder.Entity("Silk.Data.Entities.LoggingChannelEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<ulong>("ChannelID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<ulong>("WebhookID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("webhook_id");

                    b.Property<string>("WebhookToken")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("webhook_token");

                    b.HasKey("Id");

                    b.ToTable("logging_channels");
                });

            modelBuilder.Entity("Silk.Data.Entities.ReminderEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<ulong>("ChannelID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTimeOffset>("ExpiresAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expires_at");

                    b.Property<ulong?>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<string>("MessageContent")
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<ulong?>("MessageID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("message_id");

                    b.Property<ulong>("OwnerID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("owner_id");

                    b.Property<ulong?>("ReplyAuthorID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("reply_author_id");

                    b.Property<ulong?>("ReplyID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("reply_message_id");

                    b.Property<string>("ReplyMessageContent")
                        .HasColumnType("text")
                        .HasColumnName("reply_content");

                    b.HasKey("Id");

                    b.ToTable("reminders");
                });

            modelBuilder.Entity("Silk.Data.Entities.TagEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<ulong?>("GuildEntityID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int?>("OriginalTagId")
                        .HasColumnType("integer")
                        .HasColumnName("parent_id");

                    b.Property<ulong>("OwnerID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("owner_id");

                    b.Property<int>("Uses")
                        .HasColumnType("integer")
                        .HasColumnName("uses");

                    b.HasKey("Id");

                    b.HasIndex("GuildEntityID");

                    b.HasIndex("OriginalTagId");

                    b.ToTable("guild_tags");
                });

            modelBuilder.Entity("Silk.Data.Entities.UserEntity", b =>
                {
                    b.Property<ulong>("ID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("id");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<int>("Flags")
                        .HasColumnType("integer")
                        .HasColumnName("flags");

                    b.HasKey("ID", "GuildID");

                    b.HasIndex("GuildID");

                    b.ToTable("users");
                });

            modelBuilder.Entity("Silk.Data.Entities.UserHistoryEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<DateTimeOffset>("JoinDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("initial_join_date");

                    b.Property<List<DateTimeOffset>>("JoinDates")
                        .IsRequired()
                        .HasColumnType("timestamp with time zone[]")
                        .HasColumnName("join_dates");

                    b.Property<List<DateTimeOffset>>("LeaveDates")
                        .IsRequired()
                        .HasColumnType("timestamp with time zone[]")
                        .HasColumnName("leave_dates");

                    b.Property<ulong>("UserID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserID", "GuildID")
                        .IsUnique();

                    b.ToTable("user_histories");
                });

            modelBuilder.Entity("Silk.Data.Entities.ExemptionEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildModConfigEntity", null)
                        .WithMany("Exemptions")
                        .HasForeignKey("GuildModConfigEntityId");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildConfigEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildEntity", "Guild")
                        .WithOne("Configuration")
                        .HasForeignKey("Silk.Data.Entities.GuildConfigEntity", "GuildID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildGreetingEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildConfigEntity", null)
                        .WithMany("Greetings")
                        .HasForeignKey("GuildConfigEntityId");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildLoggingConfigEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.LoggingChannelEntity", "Infractions")
                        .WithMany()
                        .HasForeignKey("InfractionsId");

                    b.HasOne("Silk.Data.Entities.LoggingChannelEntity", "MemberJoins")
                        .WithMany()
                        .HasForeignKey("MemberJoinsId");

                    b.HasOne("Silk.Data.Entities.LoggingChannelEntity", "MemberLeaves")
                        .WithMany()
                        .HasForeignKey("MemberLeavesId");

                    b.HasOne("Silk.Data.Entities.LoggingChannelEntity", "MessageDeletes")
                        .WithMany()
                        .HasForeignKey("MessageDeletesId");

                    b.HasOne("Silk.Data.Entities.LoggingChannelEntity", "MessageEdits")
                        .WithMany()
                        .HasForeignKey("MessageEditsId");

                    b.Navigation("Infractions");

                    b.Navigation("MemberJoins");

                    b.Navigation("MemberLeaves");

                    b.Navigation("MessageDeletes");

                    b.Navigation("MessageEdits");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildModConfigEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildEntity", "Guild")
                        .WithOne("ModConfig")
                        .HasForeignKey("Silk.Data.Entities.GuildModConfigEntity", "GuildID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Silk.Data.Entities.GuildLoggingConfigEntity", "LoggingConfig")
                        .WithMany()
                        .HasForeignKey("LoggingConfigId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("LoggingConfig");
                });

            modelBuilder.Entity("Silk.Data.Entities.InfractionEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildEntity", "Guild")
                        .WithMany("Infractions")
                        .HasForeignKey("GuildID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Silk.Data.Entities.UserEntity", "Target")
                        .WithMany("Infractions")
                        .HasForeignKey("TargetID", "GuildID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("Target");
                });

            modelBuilder.Entity("Silk.Data.Entities.InfractionStepEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildModConfigEntity", null)
                        .WithMany("InfractionSteps")
                        .HasForeignKey("GuildModConfigEntityId");
                });

            modelBuilder.Entity("Silk.Data.Entities.InviteEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildModConfigEntity", null)
                        .WithMany("AllowedInvites")
                        .HasForeignKey("GuildModConfigEntityId");
                });

            modelBuilder.Entity("Silk.Data.Entities.TagEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildEntity", null)
                        .WithMany("Tags")
                        .HasForeignKey("GuildEntityID");

                    b.HasOne("Silk.Data.Entities.TagEntity", "OriginalTag")
                        .WithMany("Aliases")
                        .HasForeignKey("OriginalTagId");

                    b.Navigation("OriginalTag");
                });

            modelBuilder.Entity("Silk.Data.Entities.UserEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildEntity", "Guild")
                        .WithMany("Users")
                        .HasForeignKey("GuildID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Silk.Data.Entities.UserHistoryEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.UserEntity", "User")
                        .WithOne("History")
                        .HasForeignKey("Silk.Data.Entities.UserHistoryEntity", "UserID", "GuildID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildConfigEntity", b =>
                {
                    b.Navigation("Greetings");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildEntity", b =>
                {
                    b.Navigation("Configuration")
                        .IsRequired();

                    b.Navigation("Infractions");

                    b.Navigation("ModConfig")
                        .IsRequired();

                    b.Navigation("Tags");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildModConfigEntity", b =>
                {
                    b.Navigation("AllowedInvites");

                    b.Navigation("Exemptions");

                    b.Navigation("InfractionSteps");
                });

            modelBuilder.Entity("Silk.Data.Entities.TagEntity", b =>
                {
                    b.Navigation("Aliases");
                });

            modelBuilder.Entity("Silk.Data.Entities.UserEntity", b =>
                {
                    b.Navigation("History")
                        .IsRequired();

                    b.Navigation("Infractions");
                });
#pragma warning restore 612, 618
        }
    }
}
