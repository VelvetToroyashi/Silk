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
                .AddField("Description:", ChangeLog.Description)
                .AddField("Changes:", ChangeLog.Changes);

            await ctx.RespondAsync(embed: embed);


        }

    }
}
