using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class RemoveOldTables : Migration
    {
        //Manually added. Probably not a good idea.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
@"DROP TABLE IF EXISTS
    ""CommandInvocations"",
    ""DisabledCommandEntity"",
    ""ExemptionEntity"",
    ""GuildConfigs"",
    ""GuildGreetingEntity"",
    ""GuildLoggingConfigEntity"",
    ""GuildModConfigs"",
    ""Guilds"",
    ""Infractions"",
    ""InfractionStepEntity"",
    ""InviteEntity"",
    ""LoggingChannelEntity"",
    ""Reminders"",
    ""Tags"",
    ""UserHistoryEntity"",
    ""Users""
    CASCADE;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
