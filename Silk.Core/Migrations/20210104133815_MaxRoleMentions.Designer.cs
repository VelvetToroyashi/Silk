﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Silk.Core.Database;

namespace Silk.Core.Migrations
{
    [DbContext(typeof(SilkDbContext))]
    [Migration("20210104133815_MaxRoleMentions")]
    partial class MaxRoleMentions
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Silk.Core.Database.Models.Ban", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<DateTime?>("Expiration")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("GuildId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("GuildId1")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long?>("UserInfoDatabaseId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("GuildId1");

                    b.HasIndex("UserInfoDatabaseId");

                    b.ToTable("Ban");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.BlackListedWord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("GuildConfigId")
                        .HasColumnType("integer");

                    b.Property<string>("Word")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GuildConfigId");

                    b.ToTable("BlackListedWord");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.ChangelogModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Additions")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Authors")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("ChangeTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Removals")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("ChangeLogs");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.GlobalUserModel", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Cash")
                        .HasColumnType("integer");

                    b.Property<DateTime>("LastCashOut")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("GlobalUsers");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.GuildConfigModel", b =>
                {
                    b.Property<int>("ConfigId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<bool>("AutoDehoist")
                        .HasColumnType("boolean");

                    b.Property<bool>("BlacklistInvites")
                        .HasColumnType("boolean");

                    b.Property<bool>("BlacklistWords")
                        .HasColumnType("boolean");

                    b.Property<bool>("DeleteMessageOnMatchedInvite")
                        .HasColumnType("boolean");

                    b.Property<decimal>("GeneralLoggingChannel")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("GreetMembers")
                        .HasColumnType("boolean");

                    b.Property<decimal>("GreetingChannel")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("InfractionFormat")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsPremium")
                        .HasColumnType("boolean");

                    b.Property<bool>("LogMessageChanges")
                        .HasColumnType("boolean");

                    b.Property<int>("MaxRoleMentions")
                        .HasColumnType("integer");

                    b.Property<int>("MaxUserMentions")
                        .HasColumnType("integer");

                    b.Property<decimal>("MuteRoleId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("ScanInvites")
                        .HasColumnType("boolean");

                    b.Property<bool>("UseAggressiveRegex")
                        .HasColumnType("boolean");

                    b.Property<bool>("WarnOnMatchedInvite")
                        .HasColumnType("boolean");

                    b.HasKey("ConfigId");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.ToTable("GuildConfigs");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.GuildInviteModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int?>("GuildConfigModelConfigId")
                        .HasColumnType("integer");

                    b.Property<string>("GuildName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("VanityURL")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GuildConfigModelConfigId");

                    b.ToTable("GuildInviteModel");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.GuildModel", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Prefix")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("character varying(5)");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.ItemModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<decimal>("OwnerId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("State")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.SelfAssignableRole", b =>
                {
                    b.Property<decimal>("RoleId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("GuildConfigModelConfigId")
                        .HasColumnType("integer");

                    b.HasKey("RoleId");

                    b.HasIndex("GuildConfigModelConfigId");

                    b.ToTable("SelfAssignableRole");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.TicketMessageHistoryModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Sender")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("TicketModelId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TicketModelId");

                    b.ToTable("TicketMessageHistoryModel");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.TicketModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<DateTime>("Closed")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsOpen")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("Opened")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("Opener")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("Tickets");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.TicketResponderModel", b =>
                {
                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("ResponderId")
                        .HasColumnType("numeric(20,0)");

                    b.ToTable("TicketResponderModel");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.UserInfractionModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<decimal>("Enforcer")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("InfractionTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("InfractionType")
                        .HasColumnType("integer");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("UserDatabaseId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("UserDatabaseId");

                    b.HasIndex("GuildId", "UserId");

                    b.ToTable("UserInfractionModel");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.UserModel", b =>
                {
                    b.Property<long>("DatabaseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("Flags")
                        .HasColumnType("integer");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("DatabaseId");

                    b.HasIndex("GuildId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.Ban", b =>
                {
                    b.HasOne("Silk.Core.Database.Models.GuildModel", "Guild")
                        .WithMany("Bans")
                        .HasForeignKey("GuildId1")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Silk.Core.Database.Models.UserModel", "UserInfo")
                        .WithMany()
                        .HasForeignKey("UserInfoDatabaseId");

                    b.Navigation("Guild");

                    b.Navigation("UserInfo");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.BlackListedWord", b =>
                {
                    b.HasOne("Silk.Core.Database.Models.GuildConfigModel", "Guild")
                        .WithMany("BlackListedWords")
                        .HasForeignKey("GuildConfigId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.GuildConfigModel", b =>
                {
                    b.HasOne("Silk.Core.Database.Models.GuildModel", "Guild")
                        .WithOne("Configuration")
                        .HasForeignKey("Silk.Core.Database.Models.GuildConfigModel", "GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.GuildInviteModel", b =>
                {
                    b.HasOne("Silk.Core.Database.Models.GuildConfigModel", null)
                        .WithMany("AllowedInvites")
                        .HasForeignKey("GuildConfigModelConfigId");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.ItemModel", b =>
                {
                    b.HasOne("Silk.Core.Database.Models.GlobalUserModel", "Owner")
                        .WithMany("Items")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.SelfAssignableRole", b =>
                {
                    b.HasOne("Silk.Core.Database.Models.GuildConfigModel", null)
                        .WithMany("SelfAssignableRoles")
                        .HasForeignKey("GuildConfigModelConfigId");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.TicketMessageHistoryModel", b =>
                {
                    b.HasOne("Silk.Core.Database.Models.TicketModel", "TicketModel")
                        .WithMany("History")
                        .HasForeignKey("TicketModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TicketModel");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.UserInfractionModel", b =>
                {
                    b.HasOne("Silk.Core.Database.Models.UserModel", "User")
                        .WithMany("Infractions")
                        .HasForeignKey("UserDatabaseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.UserModel", b =>
                {
                    b.HasOne("Silk.Core.Database.Models.GuildModel", "Guild")
                        .WithMany("Users")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.GlobalUserModel", b =>
                {
                    b.Navigation("Items");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.GuildConfigModel", b =>
                {
                    b.Navigation("AllowedInvites");

                    b.Navigation("BlackListedWords");

                    b.Navigation("SelfAssignableRoles");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.GuildModel", b =>
                {
                    b.Navigation("Bans");

                    b.Navigation("Configuration")
                        .IsRequired();

                    b.Navigation("Users");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.TicketModel", b =>
                {
                    b.Navigation("History");
                });

            modelBuilder.Entity("Silk.Core.Database.Models.UserModel", b =>
                {
                    b.Navigation("Infractions");
                });
#pragma warning restore 612, 618
        }
    }
}
