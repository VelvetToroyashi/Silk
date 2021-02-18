using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Migrations
{
    public partial class catAAAA : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GeneralLoggingChannel",
                table: "GuildConfigs",
                newName: "LoggingChannel");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LoggingChannel",
                table: "GuildConfigs",
                newName: "GeneralLoggingChannel");
        }
    }
}
