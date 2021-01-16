using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Migrations
{
    public partial class MaxRoleMentions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhiteListedLink");

            migrationBuilder.DropColumn(
                name: "LogRoleChange",
                table: "GuildConfigs");

            migrationBuilder.RenameColumn(
                name: "MaxMentionsInMessage",
                table: "GuildConfigs",
                newName: "MaxUserMentions");

            migrationBuilder.AddColumn<int>(
                name: "MaxRoleMentions",
                table: "GuildConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxRoleMentions",
                table: "GuildConfigs");

            migrationBuilder.RenameColumn(
                name: "MaxUserMentions",
                table: "GuildConfigs",
                newName: "MaxMentionsInMessage");

            migrationBuilder.AddColumn<bool>(
                name: "LogRoleChange",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "WhiteListedLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildConfigId = table.Column<int>(type: "integer", nullable: false),
                    GuildLevelLink = table.Column<bool>(type: "boolean", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteListedLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhiteListedLink_GuildConfigs_GuildConfigId",
                        column: x => x.GuildConfigId,
                        principalTable: "GuildConfigs",
                        principalColumn: "ConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhiteListedLink_GuildConfigId",
                table: "WhiteListedLink",
                column: "GuildConfigId");
        }
    }
}