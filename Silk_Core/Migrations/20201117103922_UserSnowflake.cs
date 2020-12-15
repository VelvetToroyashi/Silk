using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SilkBot.Migrations
{
    public partial class UserSnowflake : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_Ban_Users_UserInfoId",
                "Ban");

            migrationBuilder.DropForeignKey(
                "FK_UserInfractionModel_Users_UserId",
                "UserInfractionModel");

            migrationBuilder.DropPrimaryKey(
                "PK_Users",
                "Users");

            migrationBuilder.DropIndex(
                "IX_UserInfractionModel_UserId",
                "UserInfractionModel");

            migrationBuilder.DropIndex(
                "IX_Ban_UserInfoId",
                "Ban");

            migrationBuilder.DropColumn(
                "UserId",
                "UserInfractionModel");

            migrationBuilder.DropColumn(
                "UserInfoId",
                "Ban");

            migrationBuilder.AddColumn<long>(
                                "DatabaseId",
                                "Users",
                                "bigint",
                                nullable: false,
                                defaultValue: 0L)
                            .Annotation("Npgsql:ValueGenerationStrategy",
                                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                "UserDatabaseId",
                "UserInfractionModel",
                "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                "UserInfoDatabaseId",
                "Ban",
                "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                "PK_Users",
                "Users",
                "DatabaseId");

            migrationBuilder.CreateIndex(
                "IX_UserInfractionModel_UserDatabaseId",
                "UserInfractionModel",
                "UserDatabaseId");

            migrationBuilder.CreateIndex(
                "IX_Ban_UserInfoDatabaseId",
                "Ban",
                "UserInfoDatabaseId");

            migrationBuilder.AddForeignKey(
                "FK_Ban_Users_UserInfoDatabaseId",
                "Ban",
                "UserInfoDatabaseId",
                "Users",
                principalColumn: "DatabaseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                "FK_UserInfractionModel_Users_UserDatabaseId",
                "UserInfractionModel",
                "UserDatabaseId",
                "Users",
                principalColumn: "DatabaseId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_Ban_Users_UserInfoDatabaseId",
                "Ban");

            migrationBuilder.DropForeignKey(
                "FK_UserInfractionModel_Users_UserDatabaseId",
                "UserInfractionModel");

            migrationBuilder.DropPrimaryKey(
                "PK_Users",
                "Users");

            migrationBuilder.DropIndex(
                "IX_UserInfractionModel_UserDatabaseId",
                "UserInfractionModel");

            migrationBuilder.DropIndex(
                "IX_Ban_UserInfoDatabaseId",
                "Ban");

            migrationBuilder.DropColumn(
                "DatabaseId",
                "Users");

            migrationBuilder.DropColumn(
                "UserDatabaseId",
                "UserInfractionModel");

            migrationBuilder.DropColumn(
                "UserInfoDatabaseId",
                "Ban");

            migrationBuilder.AddColumn<decimal>(
                "UserId",
                "UserInfractionModel",
                "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                "UserInfoId",
                "Ban",
                "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                "PK_Users",
                "Users",
                "Id");

            migrationBuilder.CreateIndex(
                "IX_UserInfractionModel_UserId",
                "UserInfractionModel",
                "UserId");

            migrationBuilder.CreateIndex(
                "IX_Ban_UserInfoId",
                "Ban",
                "UserInfoId");

            migrationBuilder.AddForeignKey(
                "FK_Ban_Users_UserInfoId",
                "Ban",
                "UserInfoId",
                "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                "FK_UserInfractionModel_Users_UserId",
                "UserInfractionModel",
                "UserId",
                "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}