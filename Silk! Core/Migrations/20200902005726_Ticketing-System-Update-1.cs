using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace SilkBot.Migrations
{
    public partial class TicketingSystemUpdate1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "UserId",
                table: "DiscordUserInfoSet",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Opener = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Opened = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Closed = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketMessageHistoryModel",
                columns: table => new
                {
                    Sender = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    TicketModelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessageHistoryModel", x => x.Sender);
                    table.ForeignKey(
                        name: "FK_TicketMessageHistoryModel_Tickets_TicketModelId",
                        column: x => x.TicketModelId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketResponderModel",
                columns: table => new
                {
                    ResponderId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    TicketId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketResponderModel", x => x.ResponderId);
                    table.ForeignKey(
                        name: "FK_TicketResponderModel_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessageHistoryModel_TicketModelId",
                table: "TicketMessageHistoryModel",
                column: "TicketModelId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketResponderModel_TicketId",
                table: "TicketResponderModel",
                column: "TicketId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketMessageHistoryModel");

            migrationBuilder.DropTable(
                name: "TicketResponderModel");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "DiscordUserInfoSet",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");
        }
    }
}
