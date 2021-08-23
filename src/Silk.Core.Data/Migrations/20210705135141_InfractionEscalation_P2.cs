using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class InfractionEscalation_P2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EscalatedFromStrike",
                table: "Infractions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Infractions",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EscalatedFromStrike",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Infractions");
        }
    }
}
