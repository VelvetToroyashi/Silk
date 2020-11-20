using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SilkBot.Migrations
{
    public partial class UserSnowflake : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ban_Users_UserInfoId",
                table: "Ban");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfractionModel_Users_UserId",
                table: "UserInfractionModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserInfractionModel_UserId",
                table: "UserInfractionModel");

            migrationBuilder.DropIndex(
                name: "IX_Ban_UserInfoId",
                table: "Ban");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserInfractionModel");

            migrationBuilder.DropColumn(
                name: "UserInfoId",
                table: "Ban");

            migrationBuilder.AddColumn<long>(
                name: "DatabaseId",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "UserDatabaseId",
                table: "UserInfractionModel",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserInfoDatabaseId",
                table: "Ban",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "DatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_UserDatabaseId",
                table: "UserInfractionModel",
                column: "UserDatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Ban_UserInfoDatabaseId",
                table: "Ban",
                column: "UserInfoDatabaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ban_Users_UserInfoDatabaseId",
                table: "Ban",
                column: "UserInfoDatabaseId",
                principalTable: "Users",
                principalColumn: "DatabaseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfractionModel_Users_UserDatabaseId",
                table: "UserInfractionModel",
                column: "UserDatabaseId",
                principalTable: "Users",
                principalColumn: "DatabaseId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ban_Users_UserInfoDatabaseId",
                table: "Ban");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfractionModel_Users_UserDatabaseId",
                table: "UserInfractionModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserInfractionModel_UserDatabaseId",
                table: "UserInfractionModel");

            migrationBuilder.DropIndex(
                name: "IX_Ban_UserInfoDatabaseId",
                table: "Ban");

            migrationBuilder.DropColumn(
                name: "DatabaseId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserDatabaseId",
                table: "UserInfractionModel");

            migrationBuilder.DropColumn(
                name: "UserInfoDatabaseId",
                table: "Ban");

            migrationBuilder.AddColumn<decimal>(
                name: "UserId",
                table: "UserInfractionModel",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UserInfoId",
                table: "Ban",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_UserId",
                table: "UserInfractionModel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ban_UserInfoId",
                table: "Ban",
                column: "UserInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ban_Users_UserInfoId",
                table: "Ban",
                column: "UserInfoId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfractionModel_Users_UserId",
                table: "UserInfractionModel",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
