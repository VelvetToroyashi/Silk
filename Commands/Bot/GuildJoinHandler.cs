using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SilkBot.Bot;

namespace SilkBot.Commands.Bot
{
    public sealed class GuildJoinHandler
    {
        public GuildJoinHandler() => Instance.Client.GuildCreated += OnGuildJoin;

        private async Task OnGuildJoin(GuildCreateEventArgs e)
        {
            var allChannels = (await e.Guild.GetChannelsAsync()).OrderBy(channel => channel.Position);
            var botAsMember = await e.Guild.GetMemberAsync(Instance.Client.CurrentUser.Id);

            var firstChannel = allChannels.First(channel =>
                channel.PermissionsFor(botAsMember).HasPermission(Permissions.SendMessages) &&
                channel.Type == ChannelType.Text);

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Thank you for adding me!")
                .WithColor(new DiscordColor("94f8ff"))
                .WithThumbnail(e.Client.CurrentUser.AvatarUrl)
                .WithFooter("Silk!", e.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);

            var sb = new StringBuilder();
            sb.Append("Thank you for choosing Silk! to join your server <3\n")
                .Append("I am a relatively lightweight bot with many functions - partially in moderation, ")
                .Append("partially in games, with many more features to come!\n")
                .Append("If there's an issue, feel free to [Open an issue on GitHub](https://github.com/VelvetThePanda/Silkbot/issues), ")
                .Append("or if you're not familiar with GitHub, feel free\n")
                .Append("to message the developers directly via [p]support <your message>, where `[p]` is the prefix.\n")
                .Append($"By default, the prefix is `{SilkDefaultCommandPrefix}`, or <@{e.Client.CurrentUser.Id}>, but this can be changed by [p]prefix <your prefix here>.");

            embed.WithDescription(sb.ToString());

            await firstChannel.SendMessageAsync(embed: embed);
        }
    }
}