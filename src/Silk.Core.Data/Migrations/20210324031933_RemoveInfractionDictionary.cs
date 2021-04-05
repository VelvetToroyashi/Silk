using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class RemoveInfractionDictionary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InfractionDictionary",
                table: "GuildConfigs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int[]>(
                name: "InfractionDictionary",
                table: "GuildConfigs",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }
    }
}