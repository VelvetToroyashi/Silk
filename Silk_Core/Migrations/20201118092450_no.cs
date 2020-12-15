using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SilkBot.Migrations
{
    public partial class no : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Item",
                table => new
                {
                    Id = table.Column<int>("integer", nullable: false)
                              .Annotation("Npgsql:ValueGenerationStrategy",
                                  NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>("text", nullable: true),
                    Description = table.Column<string>("text", nullable: true),
                    Discriminator = table.Column<string>("text", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Item", x => x.Id); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Item");
        }
    }
}