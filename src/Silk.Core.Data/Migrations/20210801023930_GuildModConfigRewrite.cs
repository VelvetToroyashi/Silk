using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Core.Data.Migrations
{
    public partial class GuildModConfigRewrite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InfractionStep_GuildConfigs_ConfigId",
                table: "InfractionStep");

            migrationBuilder.DropForeignKey(
                name: "FK_Invite_GuildConfigs_GuildConfigId",
                table: "Invite");

            migrationBuilder.DropTable(
                name: "RoleMenu");

            migrationBuilder.DropTable(
                name: "RoleMenuOption");

            migrationBuilder.DropTable(
                name: "SelfAssignableRole");

            migrationBuilder.DropTable(
                name: "RoleMenuEmoji");

            migrationBuilder.DropTable(
                name: "RoleMenuMenu");

            migrationBuilder.DropColumn(
                name: "AutoDehoist",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "AutoEscalateInfractions",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "BlacklistInvites",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "BlacklistWords",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "DeleteMessageOnMatchedInvite",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "GreetOnScreeningComplete",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "GreetOnVerificationRole",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "InfractionFormat",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "LogMemberJoins",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "LogMemberLeaves",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "LogMessageChanges",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "LoggingChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "MaxRoleMentions",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "MaxUserMentions",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "MuteRoleId",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "ScanInvites",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "UseAggressiveRegex",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "WarnOnMatchedInvite",
                table: "GuildConfigs");

            migrationBuilder.RenameColumn(
                name: "GuildConfigId",
                table: "Invite",
                newName: "GuildModConfigId");

            migrationBuilder.RenameIndex(
                name: "IX_Invite_GuildConfigId",
                table: "Invite",
                newName: "IX_Invite_GuildModConfigId");

            migrationBuilder.AlterColumn<int>(
                name: "ConfigId",
                table: "InfractionStep",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "GuildModConfigId",
                table: "InfractionStep",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GuildModConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MuteRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MaxUserMentions = table.Column<int>(type: "integer", nullable: false),
                    MaxRoleMentions = table.Column<int>(type: "integer", nullable: false),
                    LoggingChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LogMessageChanges = table.Column<bool>(type: "boolean", nullable: false),
                    LogMemberJoins = table.Column<bool>(type: "boolean", nullable: false),
                    LogMemberLeaves = table.Column<bool>(type: "boolean", nullable: false),
                    BlacklistInvites = table.Column<bool>(type: "boolean", nullable: false),
                    BlacklistWords = table.Column<bool>(type: "boolean", nullable: false),
                    WarnOnMatchedInvite = table.Column<bool>(type: "boolean", nullable: false),
                    DeleteMessageOnMatchedInvite = table.Column<bool>(type: "boolean", nullable: false),
                    UseAggressiveRegex = table.Column<bool>(type: "boolean", nullable: false),
                    AutoEscalateInfractions = table.Column<bool>(type: "boolean", nullable: false),
                    AutoDehoist = table.Column<bool>(type: "boolean", nullable: false),
                    ScanInvites = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildModConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildModConfigs_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InfractionStep_GuildModConfigId",
                table: "InfractionStep",
                column: "GuildModConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildModConfigs_GuildId",
                table: "GuildModConfigs",
                column: "GuildId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InfractionStep_GuildConfigs_ConfigId",
                table: "InfractionStep",
                column: "ConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InfractionStep_GuildModConfigs_GuildModConfigId",
                table: "InfractionStep",
                column: "GuildModConfigId",
                principalTable: "GuildModConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invite_GuildModConfigs_GuildModConfigId",
                table: "Invite",
                column: "GuildModConfigId",
                principalTable: "GuildModConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InfractionStep_GuildConfigs_ConfigId",
                table: "InfractionStep");

            migrationBuilder.DropForeignKey(
                name: "FK_InfractionStep_GuildModConfigs_GuildModConfigId",
                table: "InfractionStep");

            migrationBuilder.DropForeignKey(
                name: "FK_Invite_GuildModConfigs_GuildModConfigId",
                table: "Invite");

            migrationBuilder.DropTable(
                name: "GuildModConfigs");

            migrationBuilder.DropIndex(
                name: "IX_InfractionStep_GuildModConfigId",
                table: "InfractionStep");

            migrationBuilder.DropColumn(
                name: "GuildModConfigId",
                table: "InfractionStep");

            migrationBuilder.RenameColumn(
                name: "GuildModConfigId",
                table: "Invite",
                newName: "GuildConfigId");

            migrationBuilder.RenameIndex(
                name: "IX_Invite_GuildModConfigId",
                table: "Invite",
                newName: "IX_Invite_GuildConfigId");

            migrationBuilder.AlterColumn<int>(
                name: "ConfigId",
                table: "InfractionStep",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoDehoist",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoEscalateInfractions",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BlacklistInvites",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BlacklistWords",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeleteMessageOnMatchedInvite",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GreetOnScreeningComplete",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GreetOnVerificationRole",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "InfractionFormat",
                table: "GuildConfigs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "LogMemberJoins",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LogMemberLeaves",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LogMessageChanges",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LoggingChannel",
                table: "GuildConfigs",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaxRoleMentions",
                table: "GuildConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxUserMentions",
                table: "GuildConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MuteRoleId",
                table: "GuildConfigs",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "ScanInvites",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseAggressiveRegex",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WarnOnMatchedInvite",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RoleMenu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildConfigId = table.Column<int>(type: "integer", nullable: false),
                    MessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RoleDictionary = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMenu_GuildConfigs_GuildConfigId",
                        column: x => x.GuildConfigId,
                        principalTable: "GuildConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenuEmoji",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Unicode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenuEmoji", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenuMenu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    GuildConfigId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenuMenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMenuMenu_GuildConfigs_GuildConfigId",
                        column: x => x.GuildConfigId,
                        principalTable: "GuildConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SelfAssignableRole",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildConfigId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssignableRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SelfAssignableRole_GuildConfigs_GuildConfigId",
                        column: x => x.GuildConfigId,
                        principalTable: "GuildConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenuOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmojiId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    RoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RoleMenuMenuId = table.Column<int>(type: "integer", nullable: true),
                    RoleName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenuOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMenuOption_RoleMenuEmoji_EmojiId",
                        column: x => x.EmojiId,
                        principalTable: "RoleMenuEmoji",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleMenuOption_RoleMenuMenu_RoleMenuMenuId",
                        column: x => x.RoleMenuMenuId,
                        principalTable: "RoleMenuMenu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenu_GuildConfigId",
                table: "RoleMenu",
                column: "GuildConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuMenu_GuildConfigId",
                table: "RoleMenuMenu",
                column: "GuildConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuOption_EmojiId",
                table: "RoleMenuOption",
                column: "EmojiId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuOption_RoleMenuMenuId",
                table: "RoleMenuOption",
                column: "RoleMenuMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssignableRole_GuildConfigId",
                table: "SelfAssignableRole",
                column: "GuildConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_InfractionStep_GuildConfigs_ConfigId",
                table: "InfractionStep",
                column: "ConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invite_GuildConfigs_GuildConfigId",
                table: "Invite",
                column: "GuildConfigId",
                principalTable: "GuildConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
