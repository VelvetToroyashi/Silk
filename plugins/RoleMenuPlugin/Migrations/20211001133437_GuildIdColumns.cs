using Microsoft.EntityFrameworkCore.Migrations;

namespace RoleMenuPlugin.Migrations
{
    public partial class GuildIdColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "RoleMenus",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "RoleMenuOptionModel",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "RoleMenus");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "RoleMenuOptionModel");
        }
    }
}
