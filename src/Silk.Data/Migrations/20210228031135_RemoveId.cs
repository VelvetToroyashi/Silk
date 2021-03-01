using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Data.Migrations
{
    public partial class RemoveId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigId1",
                table: "SelfAssignableRole");

            migrationBuilder.DropIndex(
                name: "IX_SelfAssignableRole_GuildConfigId1",
                table: "SelfAssignableRole");

            migrationBuilder.DropColumn(
                name: "GuildConfigId1",
                table: "SelfAssignableRole");

            migrationBuilder.AlterColumn<int>(
                name: "GuildConfigId",
                table: "SelfAssignableRole",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "Infractions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "Handled",
                table: "Infractions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssignableRole_GuildConfigId",
                table: "SelfAssignableRole",
                column: "GuildConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_GuildId",
                table: "Infractions",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_Guilds_GuildId",
                table: "Infractions",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigId",
                table: "SelfAssignableRole",
                column: "GuildConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_Guilds_GuildId",
                table: "Infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigId",
                table: "SelfAssignableRole");

            migrationBuilder.DropIndex(
                name: "IX_SelfAssignableRole_GuildConfigId",
                table: "SelfAssignableRole");

            migrationBuilder.DropIndex(
                name: "IX_Infractions_GuildId",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "Handled",
                table: "Infractions");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildConfigId",
                table: "SelfAssignableRole",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuildConfigId1",
                table: "SelfAssignableRole",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssignableRole_GuildConfigId1",
                table: "SelfAssignableRole",
                column: "GuildConfigId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigId1",
                table: "SelfAssignableRole",
                column: "GuildConfigId1",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
