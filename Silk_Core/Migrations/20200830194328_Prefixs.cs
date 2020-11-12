using Microsoft.EntityFrameworkCore.Migrations;

namespace SilkBot.Migrations
{
    public partial class Prefixs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "Guilds",
                newName: "Id");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscordGuildId",
                table: "Guilds",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Prefix",
                table: "Guilds",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prefix",
                table: "Guilds");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Guilds",
                newName: "GuildId");

            migrationBuilder.AlterColumn<int>(
                name: "DiscordGuildId",
                table: "Guilds",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");
        }
    }
}
