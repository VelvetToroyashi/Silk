using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Data.Migrations
{
    public partial class LoggingConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoggingConfigEntityId",
                table: "GuildModConfigs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LoggingChannelEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    webhook_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    webhook_token = table.Column<string>(type: "text", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoggingChannelEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuildLoggingConfigEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    fallback_logging_channel = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    use_webhook_logging = table.Column<bool>(type: "boolean", nullable: false),
                    InfractionsId = table.Column<int>(type: "integer", nullable: true),
                    MessageEditsId = table.Column<int>(type: "integer", nullable: true),
                    MessageDeletesId = table.Column<int>(type: "integer", nullable: true),
                    MemberJoinsId = table.Column<int>(type: "integer", nullable: true),
                    MemberLeavesId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildLoggingConfigEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_InfractionsId",
                        column: x => x.InfractionsId,
                        principalTable: "LoggingChannelEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_MemberJoinsId",
                        column: x => x.MemberJoinsId,
                        principalTable: "LoggingChannelEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_MemberLeavesId",
                        column: x => x.MemberLeavesId,
                        principalTable: "LoggingChannelEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_MessageDelete~",
                        column: x => x.MessageDeletesId,
                        principalTable: "LoggingChannelEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuildLoggingConfigEntity_LoggingChannelEntity_MessageEditsId",
                        column: x => x.MessageEditsId,
                        principalTable: "LoggingChannelEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildModConfigs_LoggingConfigEntityId",
                table: "GuildModConfigs",
                column: "LoggingConfigEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildLoggingConfigEntity_InfractionsId",
                table: "GuildLoggingConfigEntity",
                column: "InfractionsId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildLoggingConfigEntity_MemberJoinsId",
                table: "GuildLoggingConfigEntity",
                column: "MemberJoinsId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildLoggingConfigEntity_MemberLeavesId",
                table: "GuildLoggingConfigEntity",
                column: "MemberLeavesId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildLoggingConfigEntity_MessageDeletesId",
                table: "GuildLoggingConfigEntity",
                column: "MessageDeletesId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildLoggingConfigEntity_MessageEditsId",
                table: "GuildLoggingConfigEntity",
                column: "MessageEditsId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildModConfigs_GuildLoggingConfigEntity_LoggingConfigEntit~",
                table: "GuildModConfigs",
                column: "LoggingConfigEntityId",
                principalTable: "GuildLoggingConfigEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildModConfigs_GuildLoggingConfigEntity_LoggingConfigEntit~",
                table: "GuildModConfigs");

            migrationBuilder.DropTable(
                name: "GuildLoggingConfigEntity");

            migrationBuilder.DropTable(
                name: "LoggingChannelEntity");

            migrationBuilder.DropIndex(
                name: "IX_GuildModConfigs_LoggingConfigEntityId",
                table: "GuildModConfigs");

            migrationBuilder.DropColumn(
                name: "LoggingConfigEntityId",
                table: "GuildModConfigs");
        }
    }
}
