using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Data.Migrations
{
    public partial class SlashRoleMenuCommands : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleMenuEmoji",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Unicode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenuEmoji", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenuMenu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildConfigId = table.Column<int>(type: "integer", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenuMenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMenuMenu_GuildConfigs_GuildConfigId",
                        column: x => x.GuildConfigId,
                        principalTable: "GuildConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenuOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    EmojiId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    RoleMenuMenuId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenuOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMenuOption_RoleMenuEmoji_EmojiId",
                        column: x => x.EmojiId,
                        principalTable: "RoleMenuEmoji",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleMenuOption_RoleMenuMenu_RoleMenuMenuId",
                        column: x => x.RoleMenuMenuId,
                        principalTable: "RoleMenuMenu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuMenu_GuildConfigId",
                table: "RoleMenuMenu",
                column: "GuildConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuOption_EmojiId",
                table: "RoleMenuOption",
                column: "EmojiId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuOption_RoleMenuMenuId",
                table: "RoleMenuOption",
                column: "RoleMenuMenuId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleMenuOption");

            migrationBuilder.DropTable(
                name: "RoleMenuEmoji");

            migrationBuilder.DropTable(
                name: "RoleMenuMenu");
        }
    }
}
