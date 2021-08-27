using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Api.Data.Migrations
{
    public partial class Add_Infraction_Type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Infractions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Infractions");
        }
    }
}
