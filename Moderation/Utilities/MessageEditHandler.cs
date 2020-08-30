using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SilkBot.ServerConfigurations;
using System;
using System.Threading.Tasks;
namespace SilkBot.Commands.Moderation.Utilities
{
    public sealed class MessageEditHandler
    {
        private readonly DataStorageContainer _guildInformation;

        public MessageEditHandler(ref DataStorageContainer guildData, DiscordClient client)
        {
            _guildInformation = guildData;
            client.MessageUpdated += OnMessageEdit;
        }

        private async Task OnMessageEdit(MessageUpdateEventArgs e)
        {
            if (e.Message.Author is null) return;
            if (e.MessageBefore.Content == e.Message.Content) return;
            var logChannel = _guildInformation[e.Guild].GuildInfo.LoggingChannel;
            if (e.Message.Author.IsCurrent || e.Message.Author.IsBot ) return;
            if (logChannel == default) return;
            var embed =
                new DiscordEmbedBuilder()
                .WithAuthor($"{e.Message.Author.Username} ({e.Message.Author.Id})", iconUrl: e.Message.Author.AvatarUrl)
                .WithDescription($"[Message edited in]({e.Message.JumpLink}) {e.Message.Channel.Mention}:\n" +
                $"Time: {DateTime.Now:HH:mm}\n" +
                $"📝 **Original:**\n```\n{e.MessageBefore.Content}\n```\n" +
                $"📝 **Changed:**\n```\n{e.Message.Content}\n```\n")
                .AddField("Message ID:", e.Message.Id.ToString(), true)
                .AddField("Channel ID:", e.Channel.Id.ToString(), true)
                .WithColor(DiscordColor.CornflowerBlue)
                .WithFooter("Silk!", e.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);
            var loggingChannel = await e.Client.GetChannelAsync(logChannel);
            await e.Client.SendMessageAsync(loggingChannel, embed: embed);
        }
    }
}
