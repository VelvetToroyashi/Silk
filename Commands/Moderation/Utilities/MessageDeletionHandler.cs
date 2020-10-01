using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Linq;
using System.Threading.Tasks;
using static SilkBot.Bot;

namespace SilkBot.Commands.Moderation.Utilities
{
    public class MessageDeletionHandler
    {
        public static int UnloggedMessages { get; set; }
        public MessageDeletionHandler(DiscordClient client)
        {

            client.MessageDeleted += OnMessageDeleted;
        }
        
        private async Task OnMessageDeleted(MessageDeleteEventArgs e)
        {

            if(UnloggedMessages - 1 > 0)
            {
                UnloggedMessages--;
                e.Handled = true;
                return;
            }

            if (e.Channel.IsPrivate || e.Message.Author.IsCurrent)
            {
                return;
            }

            var config = Instance.SilkDBContext.Guilds.First(g => g.DiscordGuildId == e.Guild.Id);

            if (!config.LogMessageChanges)
            {
                return;
            }

            var logChannel = config.MessageEditChannel!.Value;
            if (logChannel == default)
            {
                return;
            }
          

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
