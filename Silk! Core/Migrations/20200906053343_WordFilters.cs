using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SilkBot.Migrations
{
    public partial class WordFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BlacklistWords",
                table: "Guilds",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BlackListedWord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<int>(type: "integer", nullable: true),
                    Word = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackListedWord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlackListedWord_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlackListedWord_GuildId",
                table: "BlackListedWord",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlackListedWord");

            migrationBuilder.DropColumn(
                name: "BlacklistWords",
                table: "Guilds");
        }
    }
}
