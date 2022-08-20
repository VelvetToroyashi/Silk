using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoleMenuPlugin.Migrations
{
    public partial class MenuDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RoleMenus",
                type: "text",
                nullable: true,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "RoleMenus");
        }
    }
}
