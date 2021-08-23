using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class DoubleFK2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_Users_UserId1_UserGuildId",
                table: "Infractions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Infractions",
                table: "Infractions");

            migrationBuilder.DropIndex(
                name: "IX_Infractions_UserId1_UserGuildId",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "UserGuildId",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Infractions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Infractions",
                table: "Infractions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_UserId_GuildId",
                table: "Infractions",
                columns: new[] { "UserId", "GuildId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_Users_UserId_GuildId",
                table: "Infractions",
                columns: new[] { "UserId", "GuildId" },
                principalTable: "Users",
                principalColumns: new[] { "Id", "GuildId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_Users_UserId_GuildId",
                table: "Infractions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Infractions",
                table: "Infractions");

            migrationBuilder.DropIndex(
                name: "IX_Infractions_UserId_GuildId",
                table: "Infractions");

            migrationBuilder.AddColumn<decimal>(
                name: "UserGuildId",
                table: "Infractions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UserId1",
                table: "Infractions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Infractions",
                table: "Infractions",
                columns: new[] { "Id", "GuildId" });

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_UserId1_UserGuildId",
                table: "Infractions",
                columns: new[] { "UserId1", "UserGuildId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_Users_UserId1_UserGuildId",
                table: "Infractions",
                columns: new[] { "UserId1", "UserGuildId" },
                principalTable: "Users",
                principalColumns: new[] { "Id", "GuildId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
