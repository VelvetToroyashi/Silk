using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Data.Migrations
{
    public partial class Tags_Count : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Uses",
                table: "Tags",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Uses",
                table: "Tags");
        }
    }
}
