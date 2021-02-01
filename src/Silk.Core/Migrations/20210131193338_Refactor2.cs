using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Migrations
{
    public partial class Refactor2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_GuildConfigs_GuildConfigModelId",
                table: "Infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_Invite_GuildConfigs_GuildConfigModelId",
                table: "Invite");

            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigModelId",
                table: "SelfAssignableRole");

            migrationBuilder.RenameColumn(
                name: "GuildConfigModelId",
                table: "SelfAssignableRole",
                newName: "GuildConfigId");

            migrationBuilder.RenameIndex(
                name: "IX_SelfAssignableRole_GuildConfigModelId",
                table: "SelfAssignableRole",
                newName: "IX_SelfAssignableRole_GuildConfigId");

            migrationBuilder.RenameColumn(
                name: "GuildConfigModelId",
                table: "Invite",
                newName: "GuildConfigId");

            migrationBuilder.RenameIndex(
                name: "IX_Invite_GuildConfigModelId",
                table: "Invite",
                newName: "IX_Invite_GuildConfigId");

            migrationBuilder.RenameColumn(
                name: "GuildConfigModelId",
                table: "Infractions",
                newName: "GuildConfigId");

            migrationBuilder.RenameIndex(
                name: "IX_Infractions_GuildConfigModelId",
                table: "Infractions",
                newName: "IX_Infractions_GuildConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_GuildConfigs_GuildConfigId",
                table: "Infractions",
                column: "GuildConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invite_GuildConfigs_GuildConfigId",
                table: "Invite",
                column: "GuildConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Infractions_GuildConfigs_GuildConfigId",
                table: "Infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_Invite_GuildConfigs_GuildConfigId",
                table: "Invite");

            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigId",
                table: "SelfAssignableRole");

            migrationBuilder.RenameColumn(
                name: "GuildConfigId",
                table: "SelfAssignableRole",
                newName: "GuildConfigModelId");

            migrationBuilder.RenameIndex(
                name: "IX_SelfAssignableRole_GuildConfigId",
                table: "SelfAssignableRole",
                newName: "IX_SelfAssignableRole_GuildConfigModelId");

            migrationBuilder.RenameColumn(
                name: "GuildConfigId",
                table: "Invite",
                newName: "GuildConfigModelId");

            migrationBuilder.RenameIndex(
                name: "IX_Invite_GuildConfigId",
                table: "Invite",
                newName: "IX_Invite_GuildConfigModelId");

            migrationBuilder.RenameColumn(
                name: "GuildConfigId",
                table: "Infractions",
                newName: "GuildConfigModelId");

            migrationBuilder.RenameIndex(
                name: "IX_Infractions_GuildConfigId",
                table: "Infractions",
                newName: "IX_Infractions_GuildConfigModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_GuildConfigs_GuildConfigModelId",
                table: "Infractions",
                column: "GuildConfigModelId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invite_GuildConfigs_GuildConfigModelId",
                table: "Invite",
                column: "GuildConfigModelId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigModelId",
                table: "SelfAssignableRole",
                column: "GuildConfigModelId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
