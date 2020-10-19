using Microsoft.EntityFrameworkCore.Migrations;

namespace SilkBot.Migrations
{
    public partial class foobar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketResponderModel_Tickets_TicketId",
                table: "TicketResponderModel");

            migrationBuilder.DropIndex(
                name: "IX_TicketResponderModel_TicketId",
                table: "TicketResponderModel");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "TicketResponderModel");

            migrationBuilder.RenameColumn(
                name: "Deletions",
                table: "ChangeLogs",
                newName: "Removals");

            migrationBuilder.AddColumn<decimal>(
                name: "ResponderId",
                table: "Tickets",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ResponderId",
                table: "Tickets",
                column: "ResponderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_TicketResponderModel_ResponderId",
                table: "Tickets",
                column: "ResponderId",
                principalTable: "TicketResponderModel",
                principalColumn: "ResponderId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_TicketResponderModel_ResponderId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ResponderId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ResponderId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "Removals",
                table: "ChangeLogs",
                newName: "Deletions");

            migrationBuilder.AddColumn<int>(
                name: "TicketId",
                table: "TicketResponderModel",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketResponderModel_TicketId",
                table: "TicketResponderModel",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketResponderModel_Tickets_TicketId",
                table: "TicketResponderModel",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
