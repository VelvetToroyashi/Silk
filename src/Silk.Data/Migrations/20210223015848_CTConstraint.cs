using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Data.Migrations
{
    public partial class CTConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DisabledCommand_GuildId",
                table: "DisabledCommand");

            migrationBuilder.CreateIndex(
                name: "IX_DisabledCommand_GuildId_CommandName",
                table: "DisabledCommand",
                columns: new[] { "GuildId", "CommandName" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DisabledCommand_GuildId_CommandName",
                table: "DisabledCommand");

            migrationBuilder.CreateIndex(
                name: "IX_DisabledCommand_GuildId",
                table: "DisabledCommand",
                column: "GuildId");
        }
    }
}
