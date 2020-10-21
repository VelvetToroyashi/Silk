using Microsoft.EntityFrameworkCore.Migrations;

namespace SilkBot.Migrations
{
    public partial class placeholdersmh : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InfractionType",
                table: "UserInfractionModel",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InfractionType",
                table: "UserInfractionModel");
        }
    }
}
