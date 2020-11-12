using Microsoft.EntityFrameworkCore.Migrations;

namespace SilkBot.Migrations
{
    public partial class no : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WhiteListInvites",
                table: "Guilds",
                newName: "WhitelistInvites");

            migrationBuilder.RenameColumn(
                name: "LogMemberJoinOrLeave",
                table: "Guilds",
                newName: "GreetMembers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WhitelistInvites",
                table: "Guilds",
                newName: "WhiteListInvites");

            migrationBuilder.RenameColumn(
                name: "GreetMembers",
                table: "Guilds",
                newName: "LogMemberJoinOrLeave");
        }
    }
}
