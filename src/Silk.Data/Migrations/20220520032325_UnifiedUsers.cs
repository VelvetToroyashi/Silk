using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class UnifiedUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_infractions_users_target_id_guild_id",
                table: "infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_user_histories_users_user_id_guild_id",
                table: "user_histories");

            migrationBuilder.DropForeignKey(
                name: "FK_users_guilds_guild_id",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_guild_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_user_histories_user_id_guild_id",
                table: "user_histories");

            migrationBuilder.DropIndex(
                name: "IX_infractions_target_id_guild_id",
                table: "infractions");

            migrationBuilder.DropColumn(
                name: "guild_id",
                table: "users");

            migrationBuilder.Sql("DELETE FROM users WHERE id IN (SELECT id FROM (SELECT id, row_number() OVER w as rnum FROM users WINDOW w AS (PARTITION BY id ORDER BY id) ) AS t WHERE t.rnum > 1);");

            migrationBuilder.Sql("TRUNCATE TABLE user_histories RESTART IDENTITY RESTRICT;");
            
            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "id");
            
            // Reconstruct any missing users based on their infractions
            migrationBuilder.Sql("INSERT INTO users(id, flags) SELECT target_id, 0 FROM infractions ON CONFLICT(id) DO NOTHING;");
            
            migrationBuilder.CreateTable(
                name: "GuildEntityUserEntity",
                columns: table => new
                {
                    GuildsID = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    UsersID = table.Column<ulong>(type: "numeric(20,0)", nullable: false)
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
                name: "IX_user_histories_user_id",
                table: "user_histories",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_infractions_target_id",
                table: "infractions",
                column: "target_id");

            migrationBuilder.CreateIndex(
                name: "IX_GuildEntityUserEntity_UsersID",
                table: "GuildEntityUserEntity",
                column: "UsersID");

            migrationBuilder.AddForeignKey(
                name: "FK_infractions_users_target_id",
                table: "infractions",
                column: "target_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_histories_users_user_id",
                table: "user_histories",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_infractions_users_target_id",
                table: "infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_user_histories_users_user_id",
                table: "user_histories");

            migrationBuilder.DropTable(
                name: "GuildEntityUserEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_user_histories_user_id",
                table: "user_histories");

            migrationBuilder.DropIndex(
                name: "IX_infractions_target_id",
                table: "infractions");

            migrationBuilder.AddColumn<decimal>(
                name: "guild_id",
                table: "users",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                columns: new[] { "id", "guild_id" });

            migrationBuilder.CreateIndex(
                name: "IX_users_guild_id",
                table: "users",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_histories_user_id_guild_id",
                table: "user_histories",
                columns: new[] { "user_id", "guild_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_infractions_target_id_guild_id",
                table: "infractions",
                columns: new[] { "target_id", "guild_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_infractions_users_target_id_guild_id",
                table: "infractions",
                columns: new[] { "target_id", "guild_id" },
                principalTable: "users",
                principalColumns: new[] { "id", "guild_id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_histories_users_user_id_guild_id",
                table: "user_histories",
                columns: new[] { "user_id", "guild_id" },
                principalTable: "users",
                principalColumns: new[] { "id", "guild_id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_guilds_guild_id",
                table: "users",
                column: "guild_id",
                principalTable: "guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
