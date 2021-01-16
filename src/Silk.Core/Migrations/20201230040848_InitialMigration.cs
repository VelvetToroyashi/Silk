using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Authors = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    Additions = table.Column<string>(type: "text", nullable: false),
                    Removals = table.Column<string>(type: "text", nullable: false),
                    ChangeTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlobalUsers",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Cash = table.Column<int>(type: "integer", nullable: false),
                    LastCashOut = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Prefix = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketResponderModel",
                columns: table => new
                {
                    ResponderId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => { });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    Opener = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Opened = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Closed = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    State = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_GlobalUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "GlobalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildConfigs",
                columns: table => new
                {
                    ConfigId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MuteRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GreetingChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GeneralLoggingChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LogMessageChanges = table.Column<bool>(type: "boolean", nullable: false),
                    GreetMembers = table.Column<bool>(type: "boolean", nullable: false),
                    LogRoleChange = table.Column<bool>(type: "boolean", nullable: false),
                    BlacklistInvites = table.Column<bool>(type: "boolean", nullable: false),
                    BlacklistWords = table.Column<bool>(type: "boolean", nullable: false),
                    UseAggressiveRegex = table.Column<bool>(type: "boolean", nullable: false),
                    IsPremium = table.Column<bool>(type: "boolean", nullable: false),
                    AutoDehoist = table.Column<bool>(type: "boolean", nullable: false),
                    ScanInvites = table.Column<bool>(type: "boolean", nullable: false),
                    InfractionFormat = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildConfigs", x => x.ConfigId);
                    table.ForeignKey(
                        name: "FK_GuildConfigs_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    DatabaseId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Flags = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.DatabaseId);
                    table.ForeignKey(
                        name: "FK_Users_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketMessageHistoryModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sender = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    TicketModelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessageHistoryModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketMessageHistoryModel_Tickets_TicketModelId",
                        column: x => x.TicketModelId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlackListedWord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildConfigId = table.Column<int>(type: "integer", nullable: false),
                    Word = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackListedWord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlackListedWord_GuildConfigs_GuildConfigId",
                        column: x => x.GuildConfigId,
                        principalTable: "GuildConfigs",
                        principalColumn: "ConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildInviteModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildName = table.Column<string>(type: "text", nullable: false),
                    VanityURL = table.Column<string>(type: "text", nullable: false),
                    GuildConfigModelConfigId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildInviteModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildInviteModel_GuildConfigs_GuildConfigModelConfigId",
                        column: x => x.GuildConfigModelConfigId,
                        principalTable: "GuildConfigs",
                        principalColumn: "ConfigId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SelfAssignableRole",
                columns: table => new
                {
                    RoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildConfigModelConfigId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssignableRole", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigModelConfigId",
                        column: x => x.GuildConfigModelConfigId,
                        principalTable: "GuildConfigs",
                        principalColumn: "ConfigId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WhiteListedLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Link = table.Column<string>(type: "text", nullable: false),
                    GuildLevelLink = table.Column<bool>(type: "boolean", nullable: false),
                    GuildConfigId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteListedLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhiteListedLink_GuildConfigs_GuildConfigId",
                        column: x => x.GuildConfigId,
                        principalTable: "GuildConfigs",
                        principalColumn: "ConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ban",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserInfoDatabaseId = table.Column<long>(type: "bigint", nullable: true),
                    GuildId1 = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ban", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ban_Guilds_GuildId1",
                        column: x => x.GuildId1,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ban_Users_UserInfoDatabaseId",
                        column: x => x.UserInfoDatabaseId,
                        principalTable: "Users",
                        principalColumn: "DatabaseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserInfractionModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Enforcer = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserDatabaseId = table.Column<long>(type: "bigint", nullable: false),
                    InfractionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    InfractionType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInfractionModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInfractionModel_Users_UserDatabaseId",
                        column: x => x.UserDatabaseId,
                        principalTable: "Users",
                        principalColumn: "DatabaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ban_GuildId1",
                table: "Ban",
                column: "GuildId1");

            migrationBuilder.CreateIndex(
                name: "IX_Ban_UserInfoDatabaseId",
                table: "Ban",
                column: "UserInfoDatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_BlackListedWord_GuildConfigId",
                table: "BlackListedWord",
                column: "GuildConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildConfigs_GuildId",
                table: "GuildConfigs",
                column: "GuildId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuildInviteModel_GuildConfigModelConfigId",
                table: "GuildInviteModel",
                column: "GuildConfigModelConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_OwnerId",
                table: "Items",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssignableRole_GuildConfigModelConfigId",
                table: "SelfAssignableRole",
                column: "GuildConfigModelConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessageHistoryModel_TicketModelId",
                table: "TicketMessageHistoryModel",
                column: "TicketModelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_GuildId_UserId",
                table: "UserInfractionModel",
                columns: new[] {"GuildId", "UserId"});

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_UserDatabaseId",
                table: "UserInfractionModel",
                column: "UserDatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GuildId",
                table: "Users",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_WhiteListedLink_GuildConfigId",
                table: "WhiteListedLink",
                column: "GuildConfigId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ban");

            migrationBuilder.DropTable(
                name: "BlackListedWord");

            migrationBuilder.DropTable(
                name: "ChangeLogs");

            migrationBuilder.DropTable(
                name: "GuildInviteModel");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "SelfAssignableRole");

            migrationBuilder.DropTable(
                name: "TicketMessageHistoryModel");

            migrationBuilder.DropTable(
                name: "TicketResponderModel");

            migrationBuilder.DropTable(
                name: "UserInfractionModel");

            migrationBuilder.DropTable(
                name: "WhiteListedLink");

            migrationBuilder.DropTable(
                name: "GlobalUsers");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "GuildConfigs");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}