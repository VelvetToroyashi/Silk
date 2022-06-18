using Microsoft.EntityFrameworkCore.Migrations;
using Silk.Data.Entities;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class AddGuildPK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE guilds ADD CONSTRAINT PK_Guilds UNIQUE (\"Id\");");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
