using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class AntiPhishing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeletePhishingLinks",
                table: "GuildModConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DetectPhishingLinks",
                table: "GuildModConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletePhishingLinks",
                table: "GuildModConfigs");

            migrationBuilder.DropColumn(
                name: "DetectPhishingLinks",
                table: "GuildModConfigs");
        }
    }
}
