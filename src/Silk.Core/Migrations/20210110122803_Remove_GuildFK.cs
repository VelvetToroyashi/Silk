using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Migrations
{
    public partial class Remove_GuildFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInfractionModel_Guilds_GuildModelId",
                table: "UserInfractionModel");

            migrationBuilder.DropIndex(
                name: "IX_UserInfractionModel_GuildId_UserId",
                table: "UserInfractionModel");

            migrationBuilder.DropIndex(
                name: "IX_UserInfractionModel_GuildModelId",
                table: "UserInfractionModel");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "UserInfractionModel");

            migrationBuilder.DropColumn(
                name: "GuildModelId",
                table: "UserInfractionModel");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_Id",
                table: "UserInfractionModel",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserInfractionModel_Id",
                table: "UserInfractionModel");

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "UserInfractionModel",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GuildModelId",
                table: "UserInfractionModel",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_GuildId_UserId",
                table: "UserInfractionModel",
                columns: new[] {"GuildId", "UserId"});

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_GuildModelId",
                table: "UserInfractionModel",
                column: "GuildModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfractionModel_Guilds_GuildModelId",
                table: "UserInfractionModel",
                column: "GuildModelId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}