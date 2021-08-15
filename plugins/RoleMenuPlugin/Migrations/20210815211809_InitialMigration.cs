using Microsoft.EntityFrameworkCore.Migrations;

namespace RoleMenuPlugin.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleMenus",
                columns: table => new
                {
                    MessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenus", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenuOption",
                columns: table => new
                {
                    MessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ComponentId = table.Column<string>(type: "text", nullable: true),
                    EmojiName = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    RoleMenuModelMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenuOption", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_RoleMenuOption_RoleMenus_RoleMenuModelMessageId",
                        column: x => x.RoleMenuModelMessageId,
                        principalTable: "RoleMenus",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuOption_RoleMenuModelMessageId",
                table: "RoleMenuOption",
                column: "RoleMenuModelMessageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleMenuOption");

            migrationBuilder.DropTable(
                name: "RoleMenus");
        }
    }
}
