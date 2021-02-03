using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Migrations
{
    public partial class AddGuildIdToInvite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "Invite",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Invite");
        }
    }
}
