using Microsoft.EntityFrameworkCore.Migrations;

namespace SilkBot.Migrations
{
    public partial class InsertUsefulnamehere : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_Guilds_GuildId",
                table: "SelfAssignableRole");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_TicketResponderModel_ResponderId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ResponderId",
                table: "Tickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketResponderModel",
                table: "TicketResponderModel");

            migrationBuilder.DropColumn(
                name: "ResponderId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "MemberLeaveJoinChannel",
                table: "Guilds");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "SelfAssignableRole",
                newName: "GuildModelId");

            migrationBuilder.RenameIndex(
                name: "IX_SelfAssignableRole_GuildId",
                table: "SelfAssignableRole",
                newName: "IX_SelfAssignableRole_GuildModelId");

            migrationBuilder.RenameColumn(
                name: "MuteRoleID",
                table: "Guilds",
                newName: "MuteRoleId");

            migrationBuilder.RenameColumn(
                name: "RoleChangeLogChannel",
                table: "Guilds",
                newName: "GreetingChannel");

            migrationBuilder.AddForeignKey(
                name: "FK_SelfAssignableRole_Guilds_GuildModelId",
                table: "SelfAssignableRole",
                column: "GuildModelId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SelfAssignableRole_Guilds_GuildModelId",
                table: "SelfAssignableRole");

            migrationBuilder.RenameColumn(
                name: "GuildModelId",
                table: "SelfAssignableRole",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_SelfAssignableRole_GuildModelId",
                table: "SelfAssignableRole",
                newName: "IX_SelfAssignableRole_GuildId");

            migrationBuilder.RenameColumn(
                name: "MuteRoleId",
                table: "Guilds",
                newName: "MuteRoleID");

            migrationBuilder.RenameColumn(
                name: "GreetingChannel",
                table: "Guilds",
                newName: "RoleChangeLogChannel");

            migrationBuilder.AddColumn<decimal>(
                name: "ResponderId",
                table: "Tickets",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberLeaveJoinChannel",
                table: "Guilds",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketResponderModel",
                table: "TicketResponderModel",
                column: "ResponderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ResponderId",
                table: "Tickets",
                column: "ResponderId");

            migrationBuilder.AddForeignKey(
                name: "FK_SelfAssignableRole_Guilds_GuildId",
                table: "SelfAssignableRole",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_TicketResponderModel_ResponderId",
                table: "Tickets",
                column: "ResponderId",
                principalTable: "TicketResponderModel",
                principalColumn: "ResponderId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
