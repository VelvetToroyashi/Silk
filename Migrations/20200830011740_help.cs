using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace SilkBot.Migrations
{
    public partial class help : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    GuildId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscordGuildId = table.Column<int>(type: "integer", nullable: false),
                    LogMessageChanges = table.Column<bool>(type: "boolean", nullable: false),
                    LogMemberJoinOrLeave = table.Column<bool>(type: "boolean", nullable: false),
                    MuteRoleID = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    MessageEditChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    GeneralLoggingChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    MemberLeaveJoinChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "DiscordUserInfoSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Cash = table.Column<int>(type: "integer", nullable: false),
                    LastCashIn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserPermissions = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUserInfoSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordUserInfoSet_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SelfAssignableRole",
                columns: table => new
                {
                    RoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssignableRole", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_SelfAssignableRole_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ban",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserInfoId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    GuildId1 = table.Column<int>(type: "integer", nullable: true),
                    GuildId = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ban", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ban_DiscordUserInfoSet_UserInfoId",
                        column: x => x.UserInfoId,
                        principalTable: "DiscordUserInfoSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ban_Guilds_GuildId1",
                        column: x => x.GuildId1,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ban_GuildId1",
                table: "Ban",
                column: "GuildId1");

            migrationBuilder.CreateIndex(
                name: "IX_Ban_UserInfoId",
                table: "Ban",
                column: "UserInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUserInfoSet_GuildId",
                table: "DiscordUserInfoSet",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssignableRole_GuildId",
                table: "SelfAssignableRole",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ban");

            migrationBuilder.DropTable(
                name: "SelfAssignableRole");

            migrationBuilder.DropTable(
                name: "DiscordUserInfoSet");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
