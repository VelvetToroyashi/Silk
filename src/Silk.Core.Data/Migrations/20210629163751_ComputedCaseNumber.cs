using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class ComputedCaseNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CaseNumber",
                table: "Infractions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaseNumber",
                table: "Infractions");
        }
    }
}
