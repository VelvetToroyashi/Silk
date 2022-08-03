using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class AntiRaid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "detect_raids",
                table: "guild_configs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "raid_decay_seconds",
                table: "guild_configs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "raid_threshold",
                table: "guild_configs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "detect_raids",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "raid_decay_seconds",
                table: "guild_configs");

            migrationBuilder.DropColumn(
                name: "raid_threshold",
                table: "guild_configs");
        }
    }
}
