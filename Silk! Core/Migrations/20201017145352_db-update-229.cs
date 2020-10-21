using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SilkBot.Migrations
{
    public partial class dbupdate229 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketMessageHistoryModel",
                table: "TicketMessageHistoryModel");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "TicketMessageHistoryModel",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketMessageHistoryModel",
                table: "TicketMessageHistoryModel",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketMessageHistoryModel",
                table: "TicketMessageHistoryModel");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TicketMessageHistoryModel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketMessageHistoryModel",
                table: "TicketMessageHistoryModel",
                column: "Sender");
        }
    }
}
