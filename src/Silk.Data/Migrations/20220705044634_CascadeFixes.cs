using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class CascadeFixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guild_tags_guilds_GuildEntityID",
                table: "guild_tags");

            migrationBuilder.DropIndex(
                name: "IX_guild_tags_GuildEntityID",
                table: "guild_tags");

            migrationBuilder.DropColumn(
                name: "GuildEntityID",
                table: "guild_tags");

            migrationBuilder.CreateIndex(
                name: "IX_guild_tags_guild_id",
                table: "guild_tags",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_guild_greetings_GuildID",
                table: "guild_greetings",
                column: "GuildID");

            migrationBuilder.AddForeignKey(
                name: "FK_guild_greetings_guilds_GuildID",
                table: "guild_greetings",
                column: "GuildID",
                principalTable: "guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_tags_guilds_guild_id",
                table: "guild_tags",
                column: "guild_id",
                principalTable: "guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guild_greetings_guilds_GuildID",
                table: "guild_greetings");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_tags_guilds_guild_id",
                table: "guild_tags");

            migrationBuilder.DropIndex(
                name: "IX_guild_tags_guild_id",
                table: "guild_tags");

            migrationBuilder.DropIndex(
                name: "IX_guild_greetings_GuildID",
                table: "guild_greetings");

            migrationBuilder.AddColumn<decimal>(
                name: "GuildEntityID",
                table: "guild_tags",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_guild_tags_GuildEntityID",
                table: "guild_tags",
                column: "GuildEntityID");

            migrationBuilder.AddForeignKey(
                name: "FK_guild_tags_guilds_GuildEntityID",
                table: "guild_tags",
                column: "GuildEntityID",
                principalTable: "guilds",
                principalColumn: "Id");
        }
    }
}
