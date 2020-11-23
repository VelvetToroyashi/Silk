using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SilkBot.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "ChangeLogs",
                table => new
                {
                    Id = table.Column<int>("integer", nullable: false)
                              .Annotation("Npgsql:ValueGenerationStrategy",
                                  NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Authors = table.Column<string>("text", nullable: true),
                    Version = table.Column<string>("text", nullable: true),
                    Additions = table.Column<string>("text", nullable: true),
                    Removals = table.Column<string>("text", nullable: true),
                    ChangeTime = table.Column<DateTime>("timestamp without time zone", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_ChangeLogs", x => x.Id); });

            migrationBuilder.CreateTable(
                "GlobalUsers",
                table => new
                {
                    Id = table.Column<decimal>("numeric(20,0)", nullable: false),
                    Cash = table.Column<int>("integer", nullable: false),
                    LastCashOut = table.Column<DateTime>("timestamp without time zone", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_GlobalUsers", x => x.Id); });

            migrationBuilder.CreateTable(
                "Guilds",
                table => new
                {
                    Id = table.Column<decimal>("numeric(20,0)", nullable: false),
                    WhitelistInvites = table.Column<bool>("boolean", nullable: false),
                    BlacklistWords = table.Column<bool>("boolean", nullable: false),
                    AutoDehoist = table.Column<bool>("boolean", nullable: false),
                    LogMessageChanges = table.Column<bool>("boolean", nullable: false),
                    GreetMembers = table.Column<bool>("boolean", nullable: false),
                    LogRoleChange = table.Column<bool>("boolean", nullable: false),
                    Prefix = table.Column<string>("character varying(5)", maxLength: 5, nullable: false),
                    InfractionFormat = table.Column<string>("text", nullable: true),
                    MuteRoleId = table.Column<decimal>("numeric(20,0)", nullable: false),
                    MessageEditChannel = table.Column<decimal>("numeric(20,0)", nullable: false),
                    GeneralLoggingChannel = table.Column<decimal>("numeric(20,0)", nullable: false),
                    GreetingChannel = table.Column<decimal>("numeric(20,0)", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Guilds", x => x.Id); });

            migrationBuilder.CreateTable(
                "TicketResponderModel",
                table => new
                {
                    ResponderId = table.Column<decimal>("numeric(20,0)", nullable: false),
                    Name = table.Column<string>("text", nullable: true)
                },
                constraints: table => { });

            migrationBuilder.CreateTable(
                "Tickets",
                table => new
                {
                    Id = table.Column<int>("integer", nullable: false)
                              .Annotation("Npgsql:ValueGenerationStrategy",
                                  NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsOpen = table.Column<bool>("boolean", nullable: false),
                    Opener = table.Column<decimal>("numeric(20,0)", nullable: false),
                    Opened = table.Column<DateTime>("timestamp without time zone", nullable: false),
                    Closed = table.Column<DateTime>("timestamp without time zone", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Tickets", x => x.Id); });

            migrationBuilder.CreateTable(
                "BlackListedWord",
                table => new
                {
                    Id = table.Column<int>("integer", nullable: false)
                              .Annotation("Npgsql:ValueGenerationStrategy",
                                  NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>("numeric(20,0)", nullable: true),
                    Word = table.Column<string>("text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackListedWord", x => x.Id);
                    table.ForeignKey(
                        "FK_BlackListedWord_Guilds_GuildId",
                        x => x.GuildId,
                        "Guilds",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "SelfAssignableRole",
                table => new
                {
                    RoleId = table.Column<decimal>("numeric(20,0)", nullable: false),
                    GuildModelId = table.Column<decimal>("numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssignableRole", x => x.RoleId);
                    table.ForeignKey(
                        "FK_SelfAssignableRole_Guilds_GuildModelId",
                        x => x.GuildModelId,
                        "Guilds",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "Users",
                table => new
                {
                    Id = table.Column<decimal>("numeric(20,0)", nullable: false),
                    GuildId = table.Column<decimal>("numeric(20,0)", nullable: true),
                    Flags = table.Column<int>("integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        "FK_Users_Guilds_GuildId",
                        x => x.GuildId,
                        "Guilds",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "WhiteListedLink",
                table => new
                {
                    Id = table.Column<int>("integer", nullable: false)
                              .Annotation("Npgsql:ValueGenerationStrategy",
                                  NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Link = table.Column<string>("text", nullable: true),
                    GuildId = table.Column<decimal>("numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteListedLink", x => x.Id);
                    table.ForeignKey(
                        "FK_WhiteListedLink_Guilds_GuildId",
                        x => x.GuildId,
                        "Guilds",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "TicketMessageHistoryModel",
                table => new
                {
                    Id = table.Column<int>("integer", nullable: false)
                              .Annotation("Npgsql:ValueGenerationStrategy",
                                  NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sender = table.Column<decimal>("numeric(20,0)", nullable: false),
                    Message = table.Column<string>("text", nullable: true),
                    TicketModelId = table.Column<int>("integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessageHistoryModel", x => x.Id);
                    table.ForeignKey(
                        "FK_TicketMessageHistoryModel_Tickets_TicketModelId",
                        x => x.TicketModelId,
                        "Tickets",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "Ban",
                table => new
                {
                    Id = table.Column<int>("integer", nullable: false)
                              .Annotation("Npgsql:ValueGenerationStrategy",
                                  NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserInfoId = table.Column<decimal>("numeric(20,0)", nullable: true),
                    GuildId1 = table.Column<decimal>("numeric(20,0)", nullable: true),
                    GuildId = table.Column<string>("text", nullable: true),
                    Reason = table.Column<string>("text", nullable: true),
                    Expiration = table.Column<DateTime>("timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ban", x => x.Id);
                    table.ForeignKey(
                        "FK_Ban_Guilds_GuildId1",
                        x => x.GuildId1,
                        "Guilds",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Ban_Users_UserInfoId",
                        x => x.UserInfoId,
                        "Users",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "UserInfractionModel",
                table => new
                {
                    Id = table.Column<int>("integer", nullable: false)
                              .Annotation("Npgsql:ValueGenerationStrategy",
                                  NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Reason = table.Column<string>("text", nullable: true),
                    Enforcer = table.Column<decimal>("numeric(20,0)", nullable: false),
                    UserId = table.Column<decimal>("numeric(20,0)", nullable: true),
                    InfractionTime = table.Column<DateTime>("timestamp without time zone", nullable: false),
                    InfractionType = table.Column<int>("integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInfractionModel", x => x.Id);
                    table.ForeignKey(
                        "FK_UserInfractionModel_Users_UserId",
                        x => x.UserId,
                        "Users",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                "IX_Ban_GuildId1",
                "Ban",
                "GuildId1");

            migrationBuilder.CreateIndex(
                "IX_Ban_UserInfoId",
                "Ban",
                "UserInfoId");

            migrationBuilder.CreateIndex(
                "IX_BlackListedWord_GuildId",
                "BlackListedWord",
                "GuildId");

            migrationBuilder.CreateIndex(
                "IX_SelfAssignableRole_GuildModelId",
                "SelfAssignableRole",
                "GuildModelId");

            migrationBuilder.CreateIndex(
                "IX_TicketMessageHistoryModel_TicketModelId",
                "TicketMessageHistoryModel",
                "TicketModelId");

            migrationBuilder.CreateIndex(
                "IX_UserInfractionModel_UserId",
                "UserInfractionModel",
                "UserId");

            migrationBuilder.CreateIndex(
                "IX_Users_GuildId",
                "Users",
                "GuildId");

            migrationBuilder.CreateIndex(
                "IX_WhiteListedLink_GuildId",
                "WhiteListedLink",
                "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Ban");

            migrationBuilder.DropTable(
                "BlackListedWord");

            migrationBuilder.DropTable(
                "ChangeLogs");

            migrationBuilder.DropTable(
                "GlobalUsers");

            migrationBuilder.DropTable(
                "SelfAssignableRole");

            migrationBuilder.DropTable(
                "TicketMessageHistoryModel");

            migrationBuilder.DropTable(
                "TicketResponderModel");

            migrationBuilder.DropTable(
                "UserInfractionModel");

            migrationBuilder.DropTable(
                "WhiteListedLink");

            migrationBuilder.DropTable(
                "Tickets");

            migrationBuilder.DropTable(
                "Users");

            migrationBuilder.DropTable(
                "Guilds");
        }
    }
}