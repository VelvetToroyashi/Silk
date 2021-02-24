using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Data.Migrations
{
    public partial class catAAAAAAAAAAAAAAAAAA_pt_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigId",
                table: "SelfAssignableRole");

            migrationBuilder.DropIndex(
                name: "IX_SelfAssignableRole_GuildConfigId",
                table: "SelfAssignableRole");

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

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssignableRole_GuildConfigId",
                table: "SelfAssignableRole",
                column: "GuildConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigId",
                table: "SelfAssignableRole",
                column: "GuildConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
