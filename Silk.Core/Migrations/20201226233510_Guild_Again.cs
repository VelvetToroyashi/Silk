using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Migrations
{
    public partial class Guild_Again : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_GuildConfigs_ConfigurationId",
                table: "Guilds");

            migrationBuilder.DropIndex(
                name: "IX_Guilds_ConfigurationId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "ConfigurationId",
                table: "Guilds");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildConfigModelId",
                table: "WhiteListedLink",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildConfigModelId",
                table: "SelfAssignableRole",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildConfigModelId",
                table: "GuildInviteModel",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Id",
                table: "GuildConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildConfigModelId",
                table: "BlackListedWord",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildConfigModelId",
                table: "Ban",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_GuildConfigs_Id",
                table: "Guilds",
                column: "Id",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_GuildConfigs_Id",
                table: "Guilds");

            migrationBuilder.AlterColumn<int>(
                name: "GuildConfigModelId",
                table: "WhiteListedLink",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GuildConfigModelId",
                table: "SelfAssignableRole",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfigurationId",
                table: "Guilds",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GuildConfigModelId",
                table: "GuildInviteModel",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "GuildConfigs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "GuildConfigModelId",
                table: "BlackListedWord",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GuildConfigModelId",
                table: "Ban",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_ConfigurationId",
                table: "Guilds",
                column: "ConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_GuildConfigs_ConfigurationId",
                table: "Guilds",
                column: "ConfigurationId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
