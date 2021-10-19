using Microsoft.EntityFrameworkCore.Migrations;

namespace Silk.Core.Data.Migrations
{
    public partial class WebhookLogging : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoggingWebhookUrl",
                table: "GuildModConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseWebhookLogging",
                table: "GuildModConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "WebhookLoggingId",
                table: "GuildModConfigs",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoggingWebhookUrl",
                table: "GuildModConfigs");

            migrationBuilder.DropColumn(
                name: "UseWebhookLogging",
                table: "GuildModConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookLoggingId",
                table: "GuildModConfigs");
        }
    }
}
