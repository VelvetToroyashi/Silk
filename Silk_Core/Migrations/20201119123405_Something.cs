using Microsoft.EntityFrameworkCore.Migrations;

namespace SilkBot.Migrations
{
    public partial class Something : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Item",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Item");

            migrationBuilder.RenameTable(
                name: "Item",
                newName: "Foobars");

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "UserInfractionModel",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UserId",
                table: "UserInfractionModel",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Foobars",
                table: "Foobars",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Foobars",
                table: "Foobars");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "UserInfractionModel");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserInfractionModel");

            migrationBuilder.RenameTable(
                name: "Foobars",
                newName: "Item");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Item",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Item",
                table: "Item",
                column: "Id");
        }
    }
}
