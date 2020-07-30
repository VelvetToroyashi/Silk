using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace SilkBot.Commands.Miscellaneous
{
    public class ChangeLogCommand : BaseCommandModule
    {
        [Command("ChangeLog")]
        public async Task GetChangeLog(CommandContext ctx)
        {
            ChangelogFile.ConvertToJSON();
            var embed = new DiscordEmbedBuilder();
            var ChangeLog = ChangelogFile.DeserializeChangeLog();
            embed.WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
                .WithColor(DiscordColor.Blue)
                .AddField("Verison", ChangeLog.Version, false)
                .AddField("What's new:", ChangeLog.ShortDescription, false)
                .AddField("Added:", ChangeLog.Additions, false)
                .AddField("Removed:", ChangeLog.Removals, false);

            await ctx.RespondAsync(embed: embed);


        }

    }
}
