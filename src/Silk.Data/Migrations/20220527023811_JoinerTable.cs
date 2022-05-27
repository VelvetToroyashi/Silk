using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class JoinerTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildEntityUserEntity");

            migrationBuilder.CreateTable(
                name: "guild_user_joiner",
                columns: table => new
                {
                    user_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_user_joiner", x => new { x.user_id, x.guild_id });
                    table.ForeignKey(
                        name: "FK_guild_user_joiner_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_guild_user_joiner_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_guild_user_joiner_guild_id",
                table: "guild_user_joiner",
                column: "guild_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guild_user_joiner");

            migrationBuilder.CreateTable(
                name: "GuildEntityUserEntity",
                columns: table => new
                {
                    GuildsID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UsersID = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildEntityUserEntity", x => new { x.GuildsID, x.UsersID });
                    table.ForeignKey(
                        name: "FK_GuildEntityUserEntity_guilds_GuildsID",
                        column: x => x.GuildsID,
                        principalTable: "guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuildEntityUserEntity_users_UsersID",
                        column: x => x.UsersID,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildEntityUserEntity_UsersID",
                table: "GuildEntityUserEntity",
                column: "UsersID");
        }
    }
}
