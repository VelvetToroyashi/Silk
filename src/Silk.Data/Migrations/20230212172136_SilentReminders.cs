using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Silk.Data.Migrations
{
    /// <inheritdoc />
    public partial class SilentReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsQuiet",
                table: "reminders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsQuiet",
                table: "reminders");
        }
    }
}
