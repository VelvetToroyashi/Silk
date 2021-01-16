using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Migrations
{
    public partial class Infraction_Update_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInfractionModel_Users_UserDatabaseId",
                table: "UserInfractionModel");

            migrationBuilder.DropTable(
                name: "Ban");

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

            migrationBuilder.AddColumn<DateTime>(
                name: "Expiration",
                table: "UserInfractionModel",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GuildModelId",
                table: "UserInfractionModel",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_GuildModelId",
                table: "UserInfractionModel",
                column: "GuildModelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_UserId",
                table: "UserInfractionModel",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfractionModel_Guilds_GuildModelId",
                table: "UserInfractionModel",
                column: "GuildModelId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfractionModel_Users_UserId",
                table: "UserInfractionModel",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInfractionModel_Guilds_GuildModelId",
                table: "UserInfractionModel");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfractionModel_Users_UserId",
                table: "UserInfractionModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserInfractionModel_GuildModelId",
                table: "UserInfractionModel");

            migrationBuilder.DropIndex(
                name: "IX_UserInfractionModel_UserId",
                table: "UserInfractionModel");

            migrationBuilder.DropColumn(
                name: "Expiration",
                table: "UserInfractionModel");

            migrationBuilder.DropColumn(
                name: "GuildModelId",
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

            migrationBuilder.CreateTable(
                name: "Ban",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GuildId = table.Column<string>(type: "text", nullable: false),
                    GuildId1 = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    UserInfoDatabaseId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ban", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ban_Guilds_GuildId1",
                        column: x => x.GuildId1,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ban_Users_UserInfoDatabaseId",
                        column: x => x.UserInfoDatabaseId,
                        principalTable: "Users",
                        principalColumn: "DatabaseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserInfractionModel_UserDatabaseId",
                table: "UserInfractionModel",
                column: "UserDatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Ban_GuildId1",
                table: "Ban",
                column: "GuildId1");

            migrationBuilder.CreateIndex(
                name: "IX_Ban_UserInfoDatabaseId",
                table: "Ban",
                column: "UserInfoDatabaseId");

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