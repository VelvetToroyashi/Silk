using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Migrations
{
    public partial class RetroActiveNonEnforcement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserInfractionModel_Id",
                table: "UserInfractionModel");

            migrationBuilder.AddColumn<bool>(
                name: "HeldAgainstUser",
                table: "UserInfractionModel",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeldAgainstUser",
                table: "UserInfractionModel");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_Id",
                table: "UserInfractionModel",
                column: "Id");
        }
    }
}