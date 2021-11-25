using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Data.Migrations
{
    public partial class Exemptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExemptionEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    exempt_from = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    target_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildModConfigEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExemptionEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExemptionEntity_GuildModConfigs_GuildModConfigEntityId",
                        column: x => x.GuildModConfigEntityId,
                        principalTable: "GuildModConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExemptionEntity_GuildModConfigEntityId",
                table: "ExemptionEntity",
                column: "GuildModConfigEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExemptionEntity");
        }
    }
}
