using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class Help : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Expiration",
                table: "InfractionStep");

            migrationBuilder.AddColumn<long>(
                name: "Duration",
                table: "InfractionStep",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "InfractionStep");

            migrationBuilder.AddColumn<DateTime>(
                name: "Expiration",
                table: "InfractionStep",
                type: "timestamp without time zone",
                nullable: true);
        }
    }
}
