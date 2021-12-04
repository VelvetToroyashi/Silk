using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Data.Migrations
{
    public partial class MultiGreetingSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "log_infractions",
                table: "GuildLoggingConfigEntity",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "log_member_joins",
                table: "GuildLoggingConfigEntity",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "log_member_leaves",
                table: "GuildLoggingConfigEntity",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "log_message_deletes",
                table: "GuildLoggingConfigEntity",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "log_message_edits",
                table: "GuildLoggingConfigEntity",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GuildGreetingEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Option = table.Column<int>(type: "integer", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MetadataSnowflake = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    GuildConfigEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildGreetingEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildGreetingEntity_GuildConfigs_GuildConfigEntityId",
                        column: x => x.GuildConfigEntityId,
                        principalTable: "GuildConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberGreetings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    when = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberGreetings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildGreetingEntity_GuildConfigEntityId",
                table: "GuildGreetingEntity",
                column: "GuildConfigEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildGreetingEntity");

            migrationBuilder.DropTable(
                name: "MemberGreetings");

            migrationBuilder.DropColumn(
                name: "log_infractions",
                table: "GuildLoggingConfigEntity");

            migrationBuilder.DropColumn(
                name: "log_member_joins",
                table: "GuildLoggingConfigEntity");

            migrationBuilder.DropColumn(
                name: "log_member_leaves",
                table: "GuildLoggingConfigEntity");

            migrationBuilder.DropColumn(
                name: "log_message_deletes",
                table: "GuildLoggingConfigEntity");

            migrationBuilder.DropColumn(
                name: "log_message_edits",
                table: "GuildLoggingConfigEntity");
        }
    }
}
