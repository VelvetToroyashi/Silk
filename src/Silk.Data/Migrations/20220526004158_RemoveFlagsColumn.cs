using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class RemoveFlagsColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE users DROP COLUMN IF EXISTS flags;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "flags",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
