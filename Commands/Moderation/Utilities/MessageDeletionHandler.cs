using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SilkBot.ServerConfigurations;
using System;
using System.Threading.Tasks;

namespace SilkBot.Commands.Moderation.Utilities
{
    public class MessageDeletionHandler
    {
        private readonly DataStorageContainer _guildInformation;
        public MessageDeletionHandler(ref DataStorageContainer guildData, DiscordClient client)
        {
            _guildInformation = guildData;
            client.MessageDeleted += OnMessageDeleted;
        }

        private async Task OnMessageDeleted(MessageDeleteEventArgs e)
        {
            var logChannel = _guildInformation[e.Guild].Guild.LoggingChannel;
            var guildPrefix = SilkBot.Bot.GuildPrefixes[e.Guild.Id];
            if (e.Message.Author.IsCurrent || e.Message.Content.StartsWith(guildPrefix)) return;
            if (logChannel == default) return;
            var embed =
                new DiscordEmbedBuilder()
                .WithTitle("Message Deleted:")
                .WithDescription(
                $"User: {e.Message.Author.Mention}\n" +
                $"Channel: {e.Channel.Mention}\n" +
                $"Time: {DateTime.Now:HH:mm}\n" +
                $"Message Contents: ```\n{e.Message.Content}```")
                .AddField("Message ID:", e.Message.Id.ToString(), true)
                .AddField("User ID:", e.Message.Author.Id.ToString(), true)
                .WithThumbnail(e.Message.Author.AvatarUrl)
                .WithColor(DiscordColor.Red)
                .WithFooter("Silk!", e.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);
            var loggingChannel = await e.Client.GetChannelAsync(logChannel);
            await e.Client.SendMessageAsync(loggingChannel, embed: embed);
        }
    }


}
