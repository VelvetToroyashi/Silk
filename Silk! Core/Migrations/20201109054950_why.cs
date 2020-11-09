using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SilkBot.Migrations
{
    public partial class why : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ban_Guilds_GuildId1",
                table: "Ban");

            migrationBuilder.DropForeignKey(
                name: "FK_BlackListedWord_Guilds_GuildId",
                table: "BlackListedWord");

            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_Guilds_GuildModelId",
                table: "SelfAssignableRole");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WhiteListedLink_Guilds_GuildId",
                table: "WhiteListedLink");

            migrationBuilder.DropIndex(
                name: "IX_WhiteListedLink_GuildId",
                table: "WhiteListedLink");

            migrationBuilder.DropIndex(
                name: "IX_Users_GuildId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_SelfAssignableRole_GuildModelId",
                table: "SelfAssignableRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Guilds",
                table: "Guilds");

            migrationBuilder.DropIndex(
                name: "IX_BlackListedWord_GuildId",
                table: "BlackListedWord");

            migrationBuilder.DropIndex(
                name: "IX_Ban_GuildId1",
                table: "Ban");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "WhiteListedLink");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GuildModelId",
                table: "SelfAssignableRole");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "BlackListedWord");

            migrationBuilder.DropColumn(
                name: "GuildId1",
                table: "Ban");

            migrationBuilder.AddColumn<decimal>(
                name: "GuildDiscordGuildId",
                table: "WhiteListedLink",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GuildDiscordGuildId",
                table: "Users",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GuildModelDiscordGuildId",
                table: "SelfAssignableRole",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GuildDiscordGuildId",
                table: "BlackListedWord",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GuildDiscordGuildId",
                table: "Ban",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Guilds",
                table: "Guilds",
                column: "DiscordGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_WhiteListedLink_GuildDiscordGuildId",
                table: "WhiteListedLink",
                column: "GuildDiscordGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GuildDiscordGuildId",
                table: "Users",
                column: "GuildDiscordGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssignableRole_GuildModelDiscordGuildId",
                table: "SelfAssignableRole",
                column: "GuildModelDiscordGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_BlackListedWord_GuildDiscordGuildId",
                table: "BlackListedWord",
                column: "GuildDiscordGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Ban_GuildDiscordGuildId",
                table: "Ban",
                column: "GuildDiscordGuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ban_Guilds_GuildDiscordGuildId",
                table: "Ban",
                column: "GuildDiscordGuildId",
                principalTable: "Guilds",
                principalColumn: "DiscordGuildId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BlackListedWord_Guilds_GuildDiscordGuildId",
                table: "BlackListedWord",
                column: "GuildDiscordGuildId",
                principalTable: "Guilds",
                principalColumn: "DiscordGuildId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SelfAssignableRole_Guilds_GuildModelDiscordGuildId",
                table: "SelfAssignableRole",
                column: "GuildModelDiscordGuildId",
                principalTable: "Guilds",
                principalColumn: "DiscordGuildId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Guilds_GuildDiscordGuildId",
                table: "Users",
                column: "GuildDiscordGuildId",
                principalTable: "Guilds",
                principalColumn: "DiscordGuildId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WhiteListedLink_Guilds_GuildDiscordGuildId",
                table: "WhiteListedLink",
                column: "GuildDiscordGuildId",
                principalTable: "Guilds",
                principalColumn: "DiscordGuildId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ban_Guilds_GuildDiscordGuildId",
                table: "Ban");

            migrationBuilder.DropForeignKey(
                name: "FK_BlackListedWord_Guilds_GuildDiscordGuildId",
                table: "BlackListedWord");

            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_Guilds_GuildModelDiscordGuildId",
                table: "SelfAssignableRole");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Guilds_GuildDiscordGuildId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WhiteListedLink_Guilds_GuildDiscordGuildId",
                table: "WhiteListedLink");

            migrationBuilder.DropIndex(
                name: "IX_WhiteListedLink_GuildDiscordGuildId",
                table: "WhiteListedLink");

            migrationBuilder.DropIndex(
                name: "IX_Users_GuildDiscordGuildId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_SelfAssignableRole_GuildModelDiscordGuildId",
                table: "SelfAssignableRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Guilds",
                table: "Guilds");

            migrationBuilder.DropIndex(
                name: "IX_BlackListedWord_GuildDiscordGuildId",
                table: "BlackListedWord");

            migrationBuilder.DropIndex(
                name: "IX_Ban_GuildDiscordGuildId",
                table: "Ban");

            migrationBuilder.DropColumn(
                name: "GuildDiscordGuildId",
                table: "WhiteListedLink");

            migrationBuilder.DropColumn(
                name: "GuildDiscordGuildId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GuildModelDiscordGuildId",
                table: "SelfAssignableRole");

            migrationBuilder.DropColumn(
                name: "GuildDiscordGuildId",
                table: "BlackListedWord");

            migrationBuilder.DropColumn(
                name: "GuildDiscordGuildId",
                table: "Ban");

            migrationBuilder.AddColumn<int>(
                name: "GuildId",
                table: "WhiteListedLink",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuildId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuildModelId",
                table: "SelfAssignableRole",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Guilds",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "GuildId",
                table: "BlackListedWord",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuildId1",
                table: "Ban",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Guilds",
                table: "Guilds",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WhiteListedLink_GuildId",
                table: "WhiteListedLink",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GuildId",
                table: "Users",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssignableRole_GuildModelId",
                table: "SelfAssignableRole",
                column: "GuildModelId");

            migrationBuilder.CreateIndex(
                name: "IX_BlackListedWord_GuildId",
                table: "BlackListedWord",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Ban_GuildId1",
                table: "Ban",
                column: "GuildId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Ban_Guilds_GuildId1",
                table: "Ban",
                column: "GuildId1",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BlackListedWord_Guilds_GuildId",
                table: "BlackListedWord",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SelfAssignableRole_Guilds_GuildModelId",
                table: "SelfAssignableRole",
                column: "GuildModelId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WhiteListedLink_Guilds_GuildId",
                table: "WhiteListedLink",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
