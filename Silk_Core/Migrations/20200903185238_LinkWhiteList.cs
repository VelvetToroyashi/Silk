using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SilkBot.Migrations
{
    public partial class LinkWhiteList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserPermissions",
                table: "DiscordUserInfoSet",
                newName: "Flags");

            migrationBuilder.AddColumn<bool>(
                name: "LogRoleChange",
                table: "Guilds",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "RoleChangeLogChannel",
                table: "Guilds",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WhiteListInvites",
                table: "Guilds",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "WhiteListedLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<int>(type: "integer", nullable: true),
                    Link = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteListedLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhiteListedLink_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhiteListedLink_GuildId",
                table: "WhiteListedLink",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhiteListedLink");

            migrationBuilder.DropColumn(
                name: "LogRoleChange",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "RoleChangeLogChannel",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "WhiteListInvites",
                table: "Guilds");

            migrationBuilder.RenameColumn(
                name: "Flags",
                table: "DiscordUserInfoSet",
                newName: "UserPermissions");
        }
    }
}
