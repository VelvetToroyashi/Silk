using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class RemoveUnusedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildModConfigs_GuildLoggingConfigEntity_LoggingConfigEntit~",
                table: "GuildModConfigs");

            migrationBuilder.DropTable(
                name: "GlobalUsers");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "CommandInvocations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CommandInvocations");

            migrationBuilder.RenameColumn(
                name: "LoggingConfigEntityId",
                table: "GuildModConfigs",
                newName: "LoggingConfigId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildModConfigs_LoggingConfigEntityId",
                table: "GuildModConfigs",
                newName: "IX_GuildModConfigs_LoggingConfigId");

            migrationBuilder.AddColumn<DateTime>(
                name: "InvocationTime",
                table: "CommandInvocations",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_GuildModConfigs_GuildLoggingConfigEntity_LoggingConfigId",
                table: "GuildModConfigs",
                column: "LoggingConfigId",
                principalTable: "GuildLoggingConfigEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildModConfigs_GuildLoggingConfigEntity_LoggingConfigId",
                table: "GuildModConfigs");

            migrationBuilder.DropColumn(
                name: "InvocationTime",
                table: "CommandInvocations");

            migrationBuilder.RenameColumn(
                name: "LoggingConfigId",
                table: "GuildModConfigs",
                newName: "LoggingConfigEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildModConfigs_LoggingConfigId",
                table: "GuildModConfigs",
                newName: "IX_GuildModConfigs_LoggingConfigEntityId");

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "CommandInvocations",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UserId",
                table: "CommandInvocations",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

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

            migrationBuilder.AddForeignKey(
                name: "FK_GuildModConfigs_GuildLoggingConfigEntity_LoggingConfigEntit~",
                table: "GuildModConfigs",
                column: "LoggingConfigEntityId",
                principalTable: "GuildLoggingConfigEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
