using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Migrations
{
    public partial class AutoMod_DeleteOnMatchedInvites : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeleteMessageOnMatchedInvite",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WarnOnMatchedInvite",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteMessageOnMatchedInvite",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "WarnOnMatchedInvite",
                table: "GuildConfigs");
        }
    }
}