using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Migrations
{
    public partial class FixConfigIdPK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlackListedWord_GuildConfigs_GuildConfigId",
                table: "BlackListedWord");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildInviteModel_GuildConfigs_GuildConfigModelConfigId",
                table: "GuildInviteModel");

            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigModelConfigId",
                table: "SelfAssignableRole");

            migrationBuilder.RenameColumn(
                name: "GuildConfigModelConfigId",
                table: "SelfAssignableRole",
                newName: "GuildConfigModelId");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "SelfAssignableRole",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_SelfAssignableRole_GuildConfigModelConfigId",
                table: "SelfAssignableRole",
                newName: "IX_SelfAssignableRole_GuildConfigModelId");

            migrationBuilder.RenameColumn(
                name: "GuildConfigModelConfigId",
                table: "GuildInviteModel",
                newName: "GuildConfigModelId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildInviteModel_GuildConfigModelConfigId",
                table: "GuildInviteModel",
                newName: "IX_GuildInviteModel_GuildConfigModelId");

            migrationBuilder.RenameColumn(
                name: "ConfigId",
                table: "GuildConfigs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "GuildConfigId",
                table: "BlackListedWord",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_BlackListedWord_GuildConfigId",
                table: "BlackListedWord",
                newName: "IX_BlackListedWord_GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlackListedWord_GuildConfigs_GuildId",
                table: "BlackListedWord",
                column: "GuildId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildInviteModel_GuildConfigs_GuildConfigModelId",
                table: "GuildInviteModel",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlackListedWord_GuildConfigs_GuildId",
                table: "BlackListedWord");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildInviteModel_GuildConfigs_GuildConfigModelId",
                table: "GuildInviteModel");

            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigModelId",
                table: "SelfAssignableRole");

            migrationBuilder.RenameColumn(
                name: "GuildConfigModelId",
                table: "SelfAssignableRole",
                newName: "GuildConfigModelConfigId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SelfAssignableRole",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_SelfAssignableRole_GuildConfigModelId",
                table: "SelfAssignableRole",
                newName: "IX_SelfAssignableRole_GuildConfigModelConfigId");

            migrationBuilder.RenameColumn(
                name: "GuildConfigModelId",
                table: "GuildInviteModel",
                newName: "GuildConfigModelConfigId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildInviteModel_GuildConfigModelId",
                table: "GuildInviteModel",
                newName: "IX_GuildInviteModel_GuildConfigModelConfigId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "GuildConfigs",
                newName: "ConfigId");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "BlackListedWord",
                newName: "GuildConfigId");

            migrationBuilder.RenameIndex(
                name: "IX_BlackListedWord_GuildId",
                table: "BlackListedWord",
                newName: "IX_BlackListedWord_GuildConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlackListedWord_GuildConfigs_GuildConfigId",
                table: "BlackListedWord",
                column: "GuildConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "ConfigId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildInviteModel_GuildConfigs_GuildConfigModelConfigId",
                table: "GuildInviteModel",
                column: "GuildConfigModelConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "ConfigId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigModelConfigId",
                table: "SelfAssignableRole",
                column: "GuildConfigModelConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "ConfigId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}