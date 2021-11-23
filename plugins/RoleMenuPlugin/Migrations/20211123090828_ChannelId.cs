using Microsoft.EntityFrameworkCore.Migrations;

namespace RoleMenuPlugin.Migrations
{
    public partial class ChannelId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ChannelId",
                table: "RoleMenus",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "EmojiName",
                table: "RoleMenuOptionModel",
                type: "text",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "RoleMenuOptionModel",
                type: "text",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ComponentId",
                table: "RoleMenuOptionModel",
                type: "text",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "RoleMenus");

            migrationBuilder.AlterColumn<string>(
                name: "EmojiName",
                table: "RoleMenuOptionModel",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "RoleMenuOptionModel",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ComponentId",
                table: "RoleMenuOptionModel",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
