using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class Update_RoleMenu_FK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleMenu_GuildConfigs_GuildId",
                table: "RoleMenu");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "RoleMenu",
                newName: "GuildConfigId");

            migrationBuilder.RenameIndex(
                name: "IX_RoleMenu_GuildId",
                table: "RoleMenu",
                newName: "IX_RoleMenu_GuildConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleMenu_GuildConfigs_GuildConfigId",
                table: "RoleMenu",
                column: "GuildConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleMenu_GuildConfigs_GuildConfigId",
                table: "RoleMenu");

            migrationBuilder.RenameColumn(
                name: "GuildConfigId",
                table: "RoleMenu",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_RoleMenu_GuildConfigId",
                table: "RoleMenu",
                newName: "IX_RoleMenu_GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleMenu_GuildConfigs_GuildId",
                table: "RoleMenu",
                column: "GuildId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
