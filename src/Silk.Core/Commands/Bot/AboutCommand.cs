using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Shared.Constants;

namespace Silk.Core.Commands.Bot
{
    [Category(Categories.Bot)]
    public class AboutCommand : BaseCommandModule
    {
        private readonly Main _main;
        public AboutCommand(Main main)
        {
            _main = main;
        }

        [Command("about")]
        [Description("Shows relevant information, data and links about Silk!")]
        public async Task SendBotInfo(CommandContext ctx)
        {
            DiscordApplication? app = await ctx.Client.GetCurrentApplicationAsync();
            Version? dsp = typeof(DiscordClient).Assembly.GetName().Version;

            int guilds = _main.ShardClient.ShardClients.Values.SelectMany(x => x.Guilds).Count();

            DiscordEmbedBuilder? embed = new DiscordEmbedBuilder()
                .WithTitle("About Silk!")
                .WithColor(DiscordColor.Gold)
                .AddField("Total guilds", $"{guilds}", true)
                .AddField("Host", Environment.MachineName, true)
                .AddField("Owner(s)", app.Owners.Select(x => x.Username).Join(", "), true)
                .AddField("Bot version", StringConstants.Version, true)
                .AddField("Library", $"DSharpPlus {dsp!.Major}.{dsp.Minor}-{dsp.Revision}", true);

            await ctx.RespondAsync(embed);
        }
    }
}