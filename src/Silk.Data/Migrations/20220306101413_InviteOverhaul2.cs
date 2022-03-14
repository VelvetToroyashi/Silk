using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class InviteOverhaul2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql
                (
                 "INSERT INTO invite_configs(\"GuildModConfigId\", \"whitelist_enabled\", \"infract\", \"delete\", \"match_aggressively\", \"scan_origin\") " +
                 "SELECT \"Id\", \"invite_whitelist_enabled\", \"infract_on_invite\", \"delete_invite_messages\", \"match_aggressively\", \"scan_invite_origin\" FROM \"temp_invites\""
                );
            
            migrationBuilder.DropTable("temp_invites");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
