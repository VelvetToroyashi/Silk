using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class InviteGuildId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InfractionStep_GuildConfigs_ConfigId",
                table: "InfractionStep");

            migrationBuilder.DropForeignKey(
                name: "FK_InfractionStep_GuildModConfigs_GuildModConfigId",
                table: "InfractionStep");

            migrationBuilder.DropIndex(
                name: "IX_InfractionStep_GuildModConfigId",
                table: "InfractionStep");

            migrationBuilder.DropColumn(
                name: "GuildModConfigId",
                table: "InfractionStep");

            migrationBuilder.AddColumn<decimal>(
                name: "InviteGuildId",
                table: "Invite",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "ConfigId",
                table: "InfractionStep",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InfractionStep_GuildModConfigs_ConfigId",
                table: "InfractionStep",
                column: "ConfigId",
                principalTable: "GuildModConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InfractionStep_GuildModConfigs_ConfigId",
                table: "InfractionStep");

            migrationBuilder.DropColumn(
                name: "InviteGuildId",
                table: "Invite");

            migrationBuilder.AlterColumn<int>(
                name: "ConfigId",
                table: "InfractionStep",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "GuildModConfigId",
                table: "InfractionStep",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InfractionStep_GuildModConfigId",
                table: "InfractionStep",
                column: "GuildModConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_InfractionStep_GuildConfigs_ConfigId",
                table: "InfractionStep",
                column: "ConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InfractionStep_GuildModConfigs_GuildModConfigId",
                table: "InfractionStep",
                column: "GuildModConfigId",
                principalTable: "GuildModConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
