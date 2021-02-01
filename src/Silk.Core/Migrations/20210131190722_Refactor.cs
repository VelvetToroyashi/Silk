using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Migrations
{
    public partial class Refactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildInfractionModel");

            migrationBuilder.DropTable(
                name: "GuildInviteModel");

            migrationBuilder.DropTable(
                name: "TicketMessageHistoryModel");

            migrationBuilder.DropTable(
                name: "TicketResponderModel");

            migrationBuilder.AddColumn<int>(
                name: "GuildConfigModelId",
                table: "Infractions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Invite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildName = table.Column<string>(type: "text", nullable: false),
                    VanityURL = table.Column<string>(type: "text", nullable: false),
                    GuildConfigModelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invite_GuildConfigs_GuildConfigModelId",
                        column: x => x.GuildConfigModelId,
                        principalTable: "GuildConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sender = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "TicketResponder",
                columns: table => new
                {
                    ResponderId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_GuildConfigModelId",
                table: "Infractions",
                column: "GuildConfigModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Invite_GuildConfigModelId",
                table: "Invite",
                column: "GuildConfigModelId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessage_TicketId",
                table: "TicketMessage",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_GuildConfigs_GuildConfigModelId",
                table: "Infractions",
                column: "GuildConfigModelId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_GuildConfigs_GuildConfigModelId",
                table: "Infractions");

            migrationBuilder.DropTable(
                name: "Invite");

            migrationBuilder.DropTable(
                name: "TicketMessage");

            migrationBuilder.DropTable(
                name: "TicketResponder");

            migrationBuilder.DropIndex(
                name: "IX_Infractions_GuildConfigModelId",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "GuildConfigModelId",
                table: "Infractions");

            migrationBuilder.CreateTable(
                name: "GuildInfractionModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfigId = table.Column<int>(type: "integer", nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildInfractionModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildInfractionModel_GuildConfigs_ConfigId",
                        column: x => x.ConfigId,
                        principalTable: "GuildConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildInviteModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildConfigModelId = table.Column<int>(type: "integer", nullable: true),
                    GuildName = table.Column<string>(type: "text", nullable: false),
                    VanityURL = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildInviteModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildInviteModel_GuildConfigs_GuildConfigModelId",
                        column: x => x.GuildConfigModelId,
                        principalTable: "GuildConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketMessageHistoryModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Sender = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TicketModelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessageHistoryModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketMessageHistoryModel_Tickets_TicketModelId",
                        column: x => x.TicketModelId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketResponderModel",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    ResponderId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildInfractionModel_ConfigId",
                table: "GuildInfractionModel",
                column: "ConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildInviteModel_GuildConfigModelId",
                table: "GuildInviteModel",
                column: "GuildConfigModelId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessageHistoryModel_TicketModelId",
                table: "TicketMessageHistoryModel",
                column: "TicketModelId");
        }
    }
}
