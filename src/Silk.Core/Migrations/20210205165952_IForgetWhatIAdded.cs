using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Silk.Core.Database.Models;

namespace Silk.Core.Migrations
{
    public partial class IForgetWhatIAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_GuildConfigs_GuildConfigId",
                table: "Infractions");

            migrationBuilder.DropIndex(
                name: "IX_Infractions_GuildConfigId",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "GuildConfigId",
                table: "Infractions");

            migrationBuilder.AddColumn<List<InfractionType>>(
                name: "InfractionDictionary",
                table: "GuildConfigs",
                type: "integer[]",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InfractionDictionary",
                table: "GuildConfigs");

            migrationBuilder.AddColumn<int>(
                name: "GuildConfigId",
                table: "Infractions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_GuildConfigId",
                table: "Infractions",
                column: "GuildConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_GuildConfigs_GuildConfigId",
                table: "Infractions",
                column: "GuildConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}