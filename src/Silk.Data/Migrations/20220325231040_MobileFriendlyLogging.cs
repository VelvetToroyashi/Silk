using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class MobileFriendlyLogging : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "use_mobile_friendly_logging",
                table: "guild_logging_configs",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "use_mobile_friendly_logging",
                table: "guild_logging_configs");
        }
    }
}
