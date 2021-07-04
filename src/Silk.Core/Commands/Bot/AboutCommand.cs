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
        private readonly DiscordShardedClient _client;
        public AboutCommand(DiscordShardedClient client) => _client = client;

        [Command("about")]
        [Description("Shows relevant information, data and links about Silk!")]
        public async Task SendBotInfo(CommandContext ctx)
        {
            DiscordApplication? app = await ctx.Client.GetCurrentApplicationAsync();
            Version? dsp = typeof(DiscordClient).Assembly.GetName().Version;

            int guilds = _client.ShardClients.Values.Sum(c => c.Guilds.Count);

            var embed = new DiscordEmbedBuilder()
                .WithTitle("About Silk!")
                .WithColor(DiscordColor.Gold)
                .AddField("Total guilds", $"{guilds}", true)
                .AddField("Owner(s)", app.Owners.Select(x => x.Username).Join(", "), true)
                .AddField("Bot version", StringConstants.Version, true)
                .AddField("Library", $"DSharpPlus {dsp!.Major}.{dsp.Minor}-{dsp.Revision}", true);


            var invite = $"https://discord.com/api/oauth2/authorize?client_id={ctx.Client.CurrentApplication.Id}&permissions=502656214&scope=bot%20applications.commands";
            var builder = new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(
                    new DiscordLinkButtonComponent(invite, "Invite me!"),
                    new DiscordLinkButtonComponent("https://github.com/VelvetThePanda/Silk", "Source Code!"),
                    new DiscordLinkButtonComponent("https://discord.gg/HZfZb95", "Support Server!"))
                .AddComponents(
                    new DiscordLinkButtonComponent("https://trello.com/b/WlPlu9CQ/the-silk-project", "Trello Board!"),
                    new DiscordLinkButtonComponent("https://ko-fi.com/velvetthepanda", "Ko-Fi! (Donations)"));
            await ctx.RespondAsync(builder);
        }
    }
}