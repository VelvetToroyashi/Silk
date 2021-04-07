using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Data.Migrations
{
    public partial class Recurring_Reminders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketMessage");

            migrationBuilder.DropTable(
                name: "TicketResponder");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Reminders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Reminders");

            migrationBuilder.CreateTable(
                name: "TicketResponder",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    ResponderId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table => { });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Closed = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    Opened = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Opener = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Sender = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TicketId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketMessage_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessage_TicketId",
                table: "TicketMessage",
                column: "TicketId");
        }
    }
}