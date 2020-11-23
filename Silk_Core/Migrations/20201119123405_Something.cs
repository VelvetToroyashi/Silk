using Microsoft.EntityFrameworkCore.Migrations;

namespace SilkBot.Migrations
{
    public partial class Something : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                "PK_Item",
                "Item");

            migrationBuilder.DropColumn(
                "Discriminator",
                "Item");

            migrationBuilder.RenameTable(
                "Item",
                newName: "Foobars");

            migrationBuilder.AddColumn<decimal>(
                "GuildId",
                "UserInfractionModel",
                "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                "UserId",
                "UserInfractionModel",
                "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                "PK_Foobars",
                "Foobars",
                "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                "PK_Foobars",
                "Foobars");

            migrationBuilder.DropColumn(
                "GuildId",
                "UserInfractionModel");

            migrationBuilder.DropColumn(
                "UserId",
                "UserInfractionModel");

            migrationBuilder.RenameTable(
                "Foobars",
                newName: "Item");

            migrationBuilder.AddColumn<string>(
                "Discriminator",
                "Item",
                "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                "PK_Item",
                "Item",
                "Id");
        }
    }
}