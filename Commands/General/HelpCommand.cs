using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SilkBot.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Commands.General
{
    public class HelpCommand : BaseCommandModule
    {
        [Command("help")]
        public async Task HelpPlusHelp(CommandContext ctx, string command = "help")
        {
            if (command == "help")
            {

                Page[] pages = ctx.Client.GetInteractivity()
                    .GeneratePagesInEmbed(HelpCache.Entries["help"].Description, SplitType.Line,
                         new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.CornflowerBlue)
                        .WithFooter($"Silk! | Requested by {ctx.User.Id}"));
                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, null);
                
                return;
            }

            if (HelpCache.Entries.TryGetValue(command.ToLower(), out var embed)) await ctx.RespondAsync(embed: embed);
        }
    }

}
