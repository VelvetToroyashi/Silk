using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class AddLogMemberLeaves : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LogMemberJoing",
                table: "GuildConfigs",
                newName: "LogMemberLeaves");

            migrationBuilder.AddColumn<bool>(
                name: "LogMemberJoins",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogMemberJoins",
                table: "GuildConfigs");

            migrationBuilder.RenameColumn(
                name: "LogMemberLeaves",
                table: "GuildConfigs",
                newName: "LogMemberJoing");
        }
    }
}
