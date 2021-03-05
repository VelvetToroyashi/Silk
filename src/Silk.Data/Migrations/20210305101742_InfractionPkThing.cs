using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Data.Migrations
{
    public partial class InfractionPkThing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_Users_UserDatabaseId",
                table: "Infractions");

            migrationBuilder.DropIndex(
                name: "IX_Infractions_UserDatabaseId",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "UserDatabaseId",
                table: "Infractions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserDatabaseId",
                table: "Infractions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_UserDatabaseId",
                table: "Infractions",
                column: "UserDatabaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_Users_UserDatabaseId",
                table: "Infractions",
                column: "UserDatabaseId",
                principalTable: "Users",
                principalColumn: "DatabaseId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
