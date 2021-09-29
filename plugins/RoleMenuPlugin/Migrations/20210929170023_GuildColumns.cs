using Microsoft.EntityFrameworkCore.Migrations;

namespace RoleMenuPlugin.Migrations
{
    public partial class GuildColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "RoleMenus",
                newName: "RM_MessageId");

            migrationBuilder.RenameColumn(
                name: "RoleMenuId",
                table: "RoleMenuOptionModel",
                newName: "RMO_FK");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "RoleMenuOptionModel",
                newName: "RMO_RoleId");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "RoleMenuOptionModel",
                newName: "RMO_MessageId");

            migrationBuilder.RenameColumn(
                name: "EmojiName",
                table: "RoleMenuOptionModel",
                newName: "RMO_Emoji");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "RoleMenuOptionModel",
                newName: "RMO_Description");

            migrationBuilder.RenameColumn(
                name: "ComponentId",
                table: "RoleMenuOptionModel",
                newName: "RMO_ComponentId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "RoleMenuOptionModel",
                newName: "RMO_Id");

            migrationBuilder.AddColumn<decimal>(
                name: "RM_GuildId",
                table: "RoleMenus",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RM_GuildId",
                table: "RoleMenus");

            migrationBuilder.RenameColumn(
                name: "RM_MessageId",
                table: "RoleMenus",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "RMO_RoleId",
                table: "RoleMenuOptionModel",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "RMO_MessageId",
                table: "RoleMenuOptionModel",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "RMO_FK",
                table: "RoleMenuOptionModel",
                newName: "RoleMenuId");

            migrationBuilder.RenameColumn(
                name: "RMO_Emoji",
                table: "RoleMenuOptionModel",
                newName: "EmojiName");

            migrationBuilder.RenameColumn(
                name: "RMO_Description",
                table: "RoleMenuOptionModel",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "RMO_ComponentId",
                table: "RoleMenuOptionModel",
                newName: "ComponentId");

            migrationBuilder.RenameColumn(
                name: "RMO_Id",
                table: "RoleMenuOptionModel",
                newName: "Id");
        }
    }
}
