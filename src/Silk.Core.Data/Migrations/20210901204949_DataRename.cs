using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Data.Migrations
{
    public partial class DataRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Guilds_GuildId",
                table: "Tags");

            migrationBuilder.DropTable(
                name: "DisabledCommand");

            migrationBuilder.DropTable(
                name: "InfractionStep");

            migrationBuilder.DropTable(
                name: "Invite");

            migrationBuilder.DropTable(
                name: "UserHistory");

            migrationBuilder.DropIndex(
                name: "IX_Tags_GuildId",
                table: "Tags");

            migrationBuilder.AddColumn<decimal>(
                name: "GuildEntityId",
                table: "Tags",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DisabledCommandEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommandName = table.Column<string>(type: "text", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildConfigEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisabledCommandEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisabledCommandEntity_GuildConfigs_GuildConfigEntityId",
                        column: x => x.GuildConfigEntityId,
                        principalTable: "GuildConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisabledCommandEntity_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InfractionStepEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfigId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Duration = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfractionStepEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfractionStepEntity_GuildModConfigs_ConfigId",
                        column: x => x.ConfigId,
                        principalTable: "GuildModConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InviteEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    InviteGuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    VanityURL = table.Column<string>(type: "text", nullable: false),
                    GuildModConfigEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InviteEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InviteEntity_GuildModConfigs_GuildModConfigEntityId",
                        column: x => x.GuildModConfigEntityId,
                        principalTable: "GuildModConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserHistoryEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    JoinDates = table.Column<List<DateTime>>(type: "timestamp without time zone[]", nullable: false),
                    LeaveDates = table.Column<List<DateTime>>(type: "timestamp without time zone[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHistoryEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHistoryEntity_Users_UserId_GuildId",
                        columns: x => new { x.UserId, x.GuildId },
                        principalTable: "Users",
                        principalColumns: new[] { "Id", "GuildId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_GuildEntityId",
                table: "Tags",
                column: "GuildEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DisabledCommandEntity_GuildConfigEntityId",
                table: "DisabledCommandEntity",
                column: "GuildConfigEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DisabledCommandEntity_GuildId_CommandName",
                table: "DisabledCommandEntity",
                columns: new[] { "GuildId", "CommandName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InfractionStepEntity_ConfigId",
                table: "InfractionStepEntity",
                column: "ConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_InviteEntity_GuildModConfigEntityId",
                table: "InviteEntity",
                column: "GuildModConfigEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistoryEntity_UserId_GuildId",
                table: "UserHistoryEntity",
                columns: new[] { "UserId", "GuildId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Guilds_GuildEntityId",
                table: "Tags",
                column: "GuildEntityId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Guilds_GuildEntityId",
                table: "Tags");

            migrationBuilder.DropTable(
                name: "DisabledCommandEntity");

            migrationBuilder.DropTable(
                name: "InfractionStepEntity");

            migrationBuilder.DropTable(
                name: "InviteEntity");

            migrationBuilder.DropTable(
                name: "UserHistoryEntity");

            migrationBuilder.DropIndex(
                name: "IX_Tags_GuildEntityId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "GuildEntityId",
                table: "Tags");

            migrationBuilder.CreateTable(
                name: "DisabledCommand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommandName = table.Column<string>(type: "text", nullable: false),
                    GuildConfigId = table.Column<int>(type: "integer", nullable: true),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisabledCommand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisabledCommand_GuildConfigs_GuildConfigId",
                        column: x => x.GuildConfigId,
                        principalTable: "GuildConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisabledCommand_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InfractionStep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfigId = table.Column<int>(type: "integer", nullable: false),
                    Duration = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfractionStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfractionStep_GuildModConfigs_ConfigId",
                        column: x => x.ConfigId,
                        principalTable: "GuildModConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildModConfigId = table.Column<int>(type: "integer", nullable: true),
                    InviteGuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    VanityURL = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invite_GuildModConfigs_GuildModConfigId",
                        column: x => x.GuildModConfigId,
                        principalTable: "GuildModConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    JoinDates = table.Column<List<DateTime>>(type: "timestamp without time zone[]", nullable: false),
                    LeaveDates = table.Column<List<DateTime>>(type: "timestamp without time zone[]", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHistory_Users_UserId_GuildId",
                        columns: x => new { x.UserId, x.GuildId },
                        principalTable: "Users",
                        principalColumns: new[] { "Id", "GuildId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_GuildId",
                table: "Tags",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_DisabledCommand_GuildConfigId",
                table: "DisabledCommand",
                column: "GuildConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_DisabledCommand_GuildId_CommandName",
                table: "DisabledCommand",
                columns: new[] { "GuildId", "CommandName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InfractionStep_ConfigId",
                table: "InfractionStep",
                column: "ConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Invite_GuildModConfigId",
                table: "Invite",
                column: "GuildModConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_UserId_GuildId",
                table: "UserHistory",
                columns: new[] { "UserId", "GuildId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Guilds_GuildId",
                table: "Tags",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
