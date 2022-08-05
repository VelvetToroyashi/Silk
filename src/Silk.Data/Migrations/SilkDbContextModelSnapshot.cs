﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Silk.Data;

#nullable disable

namespace Silk.Data.Migrations
{
    [DbContext(typeof(GuildContext))]
    partial class SilkDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
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

                    b.Property<int?>("GuildConfigEntityId")
                        .HasColumnType("integer");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<ulong>("TargetID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("target_id");

                    b.Property<int>("TargetType")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.HasIndex("GuildConfigEntityId");

                    b.ToTable("infraction_exemptions");
                });

            modelBuilder.Entity("Silk.Data.Entities.Guild.Config.InviteConfigEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("DeleteOnMatch")
                        .HasColumnType("boolean")
                        .HasColumnName("delete");

                    b.Property<int>("GuildModConfigId")
                        .HasColumnType("integer");

                    b.Property<bool>("ScanOrigin")
                        .HasColumnType("boolean")
                        .HasColumnName("scan_origin");

                    b.Property<bool>("UseAggressiveRegex")
                        .HasColumnType("boolean")
                        .HasColumnName("match_aggressively");

                    b.Property<bool>("WarnOnMatch")
                        .HasColumnType("boolean")
                        .HasColumnName("infract");

                    b.Property<bool>("WhitelistEnabled")
                        .HasColumnType("boolean")
                        .HasColumnName("whitelist_enabled");

                    b.HasKey("Id");

                    b.HasIndex("GuildModConfigId")
                        .IsUnique();

                    b.ToTable("invite_configs");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildConfigEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("BanSuspiciousUsernames")
                        .HasColumnType("boolean")
                        .HasColumnName("ban_suspicious_usernames");

                    b.Property<bool>("DeletePhishingLinks")
                        .HasColumnType("boolean")
                        .HasColumnName("delete_detected_phishing");

                    b.Property<bool>("DetectPhishingLinks")
                        .HasColumnType("boolean")
                        .HasColumnName("detect_phishing");

                    b.Property<bool>("EnableRaidDetection")
                        .HasColumnType("boolean")
                        .HasColumnName("detect_raids");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<int>("LoggingId")
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

                    b.Property<int>("RaidCooldownSeconds")
                        .HasColumnType("integer")
                        .HasColumnName("raid_decay_seconds");

                    b.Property<int>("RaidDetectionThreshold")
                        .HasColumnType("integer")
                        .HasColumnName("raid_threshold");

                    b.Property<bool>("UseNativeMute")
                        .HasColumnType("boolean")
                        .HasColumnName("use_native_mute");

                    b.HasKey("Id");

                    b.HasIndex("GuildID")
                        .IsUnique();

                    b.HasIndex("LoggingId");

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

                    b.HasIndex("GuildID");

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

                    b.Property<bool>("UseMobileFriendlyLogging")
                        .HasColumnType("boolean")
                        .HasDefaultValue(true)
                        .HasColumnName("use_mobile_friendly_logging");

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

            modelBuilder.Entity("Silk.Data.Entities.GuildUserEntity", b =>
                {
                    b.Property<ulong>("UserID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.HasKey("UserID", "GuildID");

                    b.HasIndex("GuildID");

                    b.ToTable("guild_user_joiner");
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

                    b.HasIndex("TargetID");

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

                    b.Property<int?>("GuildConfigEntityId")
                        .HasColumnType("integer");

                    b.Property<int>("Infractions")
                        .HasColumnType("integer")
                        .HasColumnName("infraction_count");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("infraction_type");

                    b.HasKey("Id");

                    b.HasIndex("GuildConfigEntityId");

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

                    b.Property<int?>("InviteConfigEntityId")
                        .HasColumnType("integer");

                    b.Property<ulong>("InviteGuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("invite_guild_id");

                    b.Property<string>("VanityURL")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("invite_code");

                    b.HasKey("Id");

                    b.HasIndex("InviteConfigEntityId");

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

            modelBuilder.Entity("Silk.Data.Entities.PendingGreetingEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("GreetingID")
                        .HasColumnType("integer")
                        .HasColumnName("greeting_id");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<ulong>("UserID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.ToTable("pending_greetings");
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

                    b.Property<bool>("IsPrivate")
                        .HasColumnType("boolean")
                        .HasColumnName("is_private");

                    b.Property<bool>("IsReply")
                        .HasColumnType("boolean")
                        .HasColumnName("is_reply");

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

                    b.Property<string>("ReplyMessageContent")
                        .HasColumnType("text")
                        .HasColumnName("reply_content");

                    b.Property<ulong?>("ReplyMessageID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("reply_message_id");

                    b.HasKey("Id");

                    b.ToTable("reminders");
                });

            modelBuilder.Entity("Silk.Data.Entities.UserEntity", b =>
                {
                    b.Property<ulong>("ID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("id");

                    b.Property<bool>("ShareTimezone")
                        .HasColumnType("boolean");

                    b.Property<string>("TimezoneID")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("users");
                });

            modelBuilder.Entity("Silk.Data.Entities.UserHistoryEntity", b =>
                {
                    b.Property<ulong>("UserID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date");

                    b.Property<bool>("IsJoin")
                        .HasColumnType("boolean")
                        .HasColumnName("is_join");

                    b.HasKey("UserID", "GuildID", "Date");

                    b.HasIndex("GuildID");

                    b.HasIndex("UserID");

                    b.ToTable("user_histories");
                });

            modelBuilder.Entity("Silk.Data.Entities.ExemptionEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildConfigEntity", null)
                        .WithMany("Exemptions")
                        .HasForeignKey("GuildConfigEntityId");
                });

            modelBuilder.Entity("Silk.Data.Entities.Guild.Config.InviteConfigEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildConfigEntity", "GuildConfig")
                        .WithOne("Invites")
                        .HasForeignKey("Silk.Data.Entities.Guild.Config.InviteConfigEntity", "GuildModConfigId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GuildConfig");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildConfigEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildEntity", "Guild")
                        .WithOne("Configuration")
                        .HasForeignKey("Silk.Data.Entities.GuildConfigEntity", "GuildID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Silk.Data.Entities.GuildLoggingConfigEntity", "Logging")
                        .WithMany()
                        .HasForeignKey("LoggingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("Logging");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildGreetingEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildConfigEntity", null)
                        .WithMany("Greetings")
                        .HasForeignKey("GuildConfigEntityId");

                    b.HasOne("Silk.Data.Entities.GuildEntity", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
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

            modelBuilder.Entity("Silk.Data.Entities.GuildUserEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildEntity", "Guild")
                        .WithMany("Users")
                        .HasForeignKey("GuildID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Silk.Data.Entities.UserEntity", "User")
                        .WithMany("Guilds")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("User");
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
                        .HasForeignKey("TargetID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("Target");
                });

            modelBuilder.Entity("Silk.Data.Entities.InfractionStepEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.GuildConfigEntity", null)
                        .WithMany("InfractionSteps")
                        .HasForeignKey("GuildConfigEntityId");
                });

            modelBuilder.Entity("Silk.Data.Entities.InviteEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.Guild.Config.InviteConfigEntity", null)
                        .WithMany("Whitelist")
                        .HasForeignKey("InviteConfigEntityId");
                });

            modelBuilder.Entity("Silk.Data.Entities.UserHistoryEntity", b =>
                {
                    b.HasOne("Silk.Data.Entities.UserEntity", "User")
                        .WithMany("History")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Silk.Data.Entities.Guild.Config.InviteConfigEntity", b =>
                {
                    b.Navigation("Whitelist");
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildConfigEntity", b =>
                {
                    b.Navigation("Exemptions");

                    b.Navigation("Greetings");

                    b.Navigation("InfractionSteps");

                    b.Navigation("Invites")
                        .IsRequired();
                });

            modelBuilder.Entity("Silk.Data.Entities.GuildEntity", b =>
                {
                    b.Navigation("Configuration")
                        .IsRequired();

                    b.Navigation("Infractions");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("Silk.Data.Entities.UserEntity", b =>
                {
                    b.Navigation("Guilds");

                    b.Navigation("History");

                    b.Navigation("Infractions");
                });
#pragma warning restore 612, 618
        }
    }
}
