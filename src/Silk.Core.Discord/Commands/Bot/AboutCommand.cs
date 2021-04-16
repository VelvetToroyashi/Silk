using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Discord.Utilities.HelpFormatter;
using Silk.Extensions;

namespace Silk.Core.Discord.Commands.Bot
{
    [Category(Categories.Bot)]
    public class AboutCommand : BaseCommandModule
    {
        [Command("about")]
        [Description("Shows relevant information, data and links about Silk!")]
        public async Task SendBotInfo(CommandContext ctx)
        {
            var app = await ctx.Client.GetCurrentApplicationAsync();
            var dsp = typeof(DiscordClient).Assembly.GetName().Version;

            int guilds = Discord.Bot.Instance!.Client.ShardClients.Values.SelectMany(x => x.Guilds).Count();

            var embed = new DiscordEmbedBuilder()
                .WithTitle("About Silk!")
                .WithColor(DiscordColor.Gold)
                .AddField("Total guilds", $"{guilds}", true)
                .AddField("Host", Environment.MachineName, true)
                .AddField("Owner(s)", app.Owners.Select(x => x.Username).Join(", "), true)
                .AddField("Bot version", Program.Version, true)
                .AddField("Library", $"DSharpPlus {dsp!.Major}.{dsp.Minor}-{dsp.Revision}", true);

            await ctx.RespondAsync(embed);
        }
    }
}