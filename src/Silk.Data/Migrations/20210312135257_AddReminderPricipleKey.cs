using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Data.Migrations
{
    public partial class AddReminderPricipleKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_Users_OwnerId",
                table: "Reminders");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_Id",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Reminders_OwnerId",
                table: "Reminders");

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
                columns: new[] { "Id", "GuildId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_OwnerId_GuildId",
                table: "Reminders",
                columns: new[] { "OwnerId", "GuildId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_Users_OwnerId_GuildId",
                table: "Reminders",
                columns: new[] { "OwnerId", "GuildId" },
                principalTable: "Users",
                principalColumns: new[] { "Id", "GuildId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_Users_OwnerId_GuildId",
                table: "Reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Reminders_OwnerId_GuildId",
                table: "Reminders");

            migrationBuilder.AlterColumn<long>(
                name: "DatabaseId",
                table: "Users",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Id",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "DatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_OwnerId",
                table: "Reminders",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_Users_OwnerId",
                table: "Reminders",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
