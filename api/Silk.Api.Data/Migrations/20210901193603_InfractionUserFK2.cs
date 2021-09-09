using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Api.Data.Migrations
{
    public partial class InfractionUserFK2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddedByFK",
                table: "Infractions",
                type: "text",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedByFK",
                table: "Infractions");
        }
    }
}
