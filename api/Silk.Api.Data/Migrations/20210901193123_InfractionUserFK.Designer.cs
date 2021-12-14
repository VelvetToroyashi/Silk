﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Silk.Api.Data;

namespace Silk.Api.Data.Migrations
{
    [DbContext(typeof(ApiContext))]
    [Migration("20210901193123_InfractionUserFK")]
    partial class InfractionUserFK
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.9")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Silk.Api.Data.Entities.ApiKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("ApiUserId")
                        .HasColumnType("integer");

                    b.Property<string>("DiscordUserId")
                        .HasColumnType("text");

                    b.Property<DateTime>("GeneratedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("ApiUserId")
                        .IsUnique();

                    b.ToTable("Keys");
                });

            modelBuilder.Entity("Silk.Api.Data.Entities.ApiUser", b =>
                {
                    b.Property<string>("DiscordId")
                        .HasColumnType("text");

                    b.Property<DateTime>("ApiKeyGenerationTimestamp")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int?>("ApiKeyId")
                        .HasColumnType("integer");

                    b.HasKey("DiscordId");

                    b.HasIndex("ApiKeyId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Silk.Api.Data.Models.InfractionEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("AddedByDiscordId")
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("EnforcerUserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime?>("Expires")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("GuildCreationId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("IsPardoned")
                        .HasColumnType("boolean");

                    b.Property<Guid>("Key")
                        .HasColumnType("uuid");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.Property<decimal>("TargetUserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("AddedByDiscordId");

                    b.ToTable("Infractions");
                });

            modelBuilder.Entity("Silk.Api.Data.Entities.ApiUser", b =>
                {
                    b.HasOne("Silk.Api.Data.Entities.ApiKey", "ApiKey")
                        .WithMany()
                        .HasForeignKey("ApiKeyId");

                    b.Navigation("ApiKey");
                });

            modelBuilder.Entity("Silk.Api.Data.Models.InfractionEntity", b =>
                {
                    b.HasOne("Silk.Api.Data.Entities.ApiUser", "AddedBy")
                        .WithMany("Infractions")
                        .HasForeignKey("AddedByDiscordId");

                    b.Navigation("AddedBy");
                });

            modelBuilder.Entity("Silk.Api.Data.Entities.ApiUser", b =>
                {
                    b.Navigation("Infractions");
                });
#pragma warning restore 612, 618
        }
    }
}