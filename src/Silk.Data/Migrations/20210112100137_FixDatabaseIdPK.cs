using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Migrations
{
    public partial class FixDatabaseIdPK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInfractionModel_Users_UserId",
                table: "UserInfractionModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserInfractionModel_UserId",
                table: "UserInfractionModel");

            migrationBuilder.AlterColumn<long>(
                    name: "DatabaseId",
                    table: "Users",
                    type: "bigint",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "UserDatabaseId",
                table: "UserInfractionModel",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "DatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_UserDatabaseId",
                table: "UserInfractionModel",
                column: "UserDatabaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfractionModel_Users_UserDatabaseId",
                table: "UserInfractionModel",
                column: "UserDatabaseId",
                principalTable: "Users",
                principalColumn: "DatabaseId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInfractionModel_Users_UserDatabaseId",
                table: "UserInfractionModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserInfractionModel_UserDatabaseId",
                table: "UserInfractionModel");

            migrationBuilder.DropColumn(
                name: "UserDatabaseId",
                table: "UserInfractionModel");

            migrationBuilder.AlterColumn<long>(
                    name: "DatabaseId",
                    table: "Users",
                    type: "bigint",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_UserId",
                table: "UserInfractionModel",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfractionModel_Users_UserId",
                table: "UserInfractionModel",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}