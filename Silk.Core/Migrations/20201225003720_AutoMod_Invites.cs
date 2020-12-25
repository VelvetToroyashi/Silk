using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Migrations
{
    public partial class AutoMod_Invites : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WhitelistInvites",
                table: "GuildConfigs",
                newName: "ScanInvites");

            migrationBuilder.AddColumn<bool>(
                name: "BlacklistInvites",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GuildInviteModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildName = table.Column<string>(type: "text", nullable: false),
                    VanityURL = table.Column<string>(type: "text", nullable: false),
                    GuildConfigModelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildInviteModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildInviteModel_GuildConfigs_GuildConfigModelId",
                        column: x => x.GuildConfigModelId,
                        principalTable: "GuildConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildInviteModel_GuildConfigModelId",
                table: "GuildInviteModel",
                column: "GuildConfigModelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildInviteModel");

            migrationBuilder.DropColumn(
                name: "BlacklistInvites",
                table: "GuildConfigs");

            migrationBuilder.RenameColumn(
                name: "ScanInvites",
                table: "GuildConfigs",
                newName: "WhitelistInvites");
        }
    }
}
