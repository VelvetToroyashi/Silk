using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Migrations
{
    public partial class Infractions_DbSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInfractionModel_Users_UserDatabaseId",
                table: "UserInfractionModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserInfractionModel",
                table: "UserInfractionModel");

            migrationBuilder.RenameTable(
                name: "UserInfractionModel",
                newName: "Infractions");

            migrationBuilder.RenameIndex(
                name: "IX_UserInfractionModel_UserDatabaseId",
                table: "Infractions",
                newName: "IX_Infractions_UserDatabaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Infractions",
                table: "Infractions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_Users_UserDatabaseId",
                table: "Infractions",
                column: "UserDatabaseId",
                principalTable: "Users",
                principalColumn: "DatabaseId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_Users_UserDatabaseId",
                table: "Infractions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Infractions",
                table: "Infractions");

            migrationBuilder.RenameTable(
                name: "Infractions",
                newName: "UserInfractionModel");

            migrationBuilder.RenameIndex(
                name: "IX_Infractions_UserDatabaseId",
                table: "UserInfractionModel",
                newName: "IX_UserInfractionModel_UserDatabaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserInfractionModel",
                table: "UserInfractionModel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfractionModel_Users_UserDatabaseId",
                table: "UserInfractionModel",
                column: "UserDatabaseId",
                principalTable: "Users",
                principalColumn: "DatabaseId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}