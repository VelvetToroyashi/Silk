using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SilkBot.Migrations
{
    public partial class StateUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ban_Guilds_GuildId",
                table: "Ban");

            migrationBuilder.DropForeignKey(
                name: "FK_BlackListedWord_Guilds_GuildId",
                table: "BlackListedWord");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_GlobalUsers_OwnerId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessageHistoryModel_Tickets_TicketModelId",
                table: "TicketMessageHistoryModel");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfractionModel_Users_UserDatabaseId",
                table: "UserInfractionModel");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WhiteListedLink_Guilds_GuildId",
                table: "WhiteListedLink");

            migrationBuilder.DropTable(
                name: "ShopItem");

            migrationBuilder.DropTable(
                name: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Ban_GuildId",
                table: "Ban");

            migrationBuilder.DropColumn(
                name: "InstanceState",
                table: "Items");

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                table: "WhiteListedLink",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "WhiteListedLink",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "Users",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserDatabaseId",
                table: "UserInfractionModel",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "UserInfractionModel",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TicketResponderModel",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "TicketModelId",
                table: "TicketMessageHistoryModel",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "TicketMessageHistoryModel",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "OwnerId",
                table: "Items",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Items",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InfractionFormat",
                table: "Guilds",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "ChangeLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Removals",
                table: "ChangeLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Authors",
                table: "ChangeLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Additions",
                table: "ChangeLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Word",
                table: "BlackListedWord",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "BlackListedWord",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<string>(
                name: "GuildId",
                table: "Ban",
                type: "text",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId1",
                table: "Ban",
                type: "numeric(20,0)",
                nullable: true);

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
                name: "FK_Items_GlobalUsers_OwnerId",
                table: "Items",
                column: "OwnerId",
                principalTable: "GlobalUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessageHistoryModel_Tickets_TicketModelId",
                table: "TicketMessageHistoryModel",
                column: "TicketModelId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfractionModel_Users_UserDatabaseId",
                table: "UserInfractionModel",
                column: "UserDatabaseId",
                principalTable: "Users",
                principalColumn: "DatabaseId",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ban_Guilds_GuildId1",
                table: "Ban");

            migrationBuilder.DropForeignKey(
                name: "FK_BlackListedWord_Guilds_GuildId",
                table: "BlackListedWord");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_GlobalUsers_OwnerId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessageHistoryModel_Tickets_TicketModelId",
                table: "TicketMessageHistoryModel");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfractionModel_Users_UserDatabaseId",
                table: "UserInfractionModel");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WhiteListedLink_Guilds_GuildId",
                table: "WhiteListedLink");

            migrationBuilder.DropIndex(
                name: "IX_Ban_GuildId1",
                table: "Ban");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "GuildId1",
                table: "Ban");

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                table: "WhiteListedLink",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "WhiteListedLink",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "Users",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "UserDatabaseId",
                table: "UserInfractionModel",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "UserInfractionModel",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TicketResponderModel",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TicketModelId",
                table: "TicketMessageHistoryModel",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "TicketMessageHistoryModel",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OwnerId",
                table: "Items",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstanceState",
                table: "Items",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "InfractionFormat",
                table: "Guilds",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "ChangeLogs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Removals",
                table: "ChangeLogs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Authors",
                table: "ChangeLogs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Additions",
                table: "ChangeLogs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Word",
                table: "BlackListedWord",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "BlackListedWord",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "Ban",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Shops",
                columns: table => new
                {
                    OwnerId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsPremium = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    ItemsSold = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shops", x => x.OwnerId);
                });

            migrationBuilder.CreateTable(
                name: "ShopItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    UserShopModelOwnerId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopItem_Shops_UserShopModelOwnerId",
                        column: x => x.UserShopModelOwnerId,
                        principalTable: "Shops",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ban_GuildId",
                table: "Ban",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopItem_UserShopModelOwnerId",
                table: "ShopItem",
                column: "UserShopModelOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ban_Guilds_GuildId",
                table: "Ban",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlackListedWord_Guilds_GuildId",
                table: "BlackListedWord",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_GlobalUsers_OwnerId",
                table: "Items",
                column: "OwnerId",
                principalTable: "GlobalUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessageHistoryModel_Tickets_TicketModelId",
                table: "TicketMessageHistoryModel",
                column: "TicketModelId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfractionModel_Users_UserDatabaseId",
                table: "UserInfractionModel",
                column: "UserDatabaseId",
                principalTable: "Users",
                principalColumn: "DatabaseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Guilds_GuildId",
                table: "Users",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WhiteListedLink_Guilds_GuildId",
                table: "WhiteListedLink",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
