using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using SilkBot.Database;
using SilkBot.Extensions;
using SilkBot.Models;

namespace SilkBot.Commands.Bot
{
    public static class BotEvents
    {
        public static async Task OnGuildJoin(DiscordClient c, GuildCreateEventArgs e)
        {
            IOrderedEnumerable<DiscordChannel> allChannels =
                (await e.Guild.GetChannelsAsync()).OrderBy(channel => channel.Position);
            DiscordMember botAsMember = await e.Guild.GetMemberAsync(c.CurrentUser.Id);

            DiscordChannel firstChannel = allChannels.First(channel =>
                channel.PermissionsFor(botAsMember).HasPermission(Permissions.SendMessages) &&
                channel.Type == ChannelType.Text);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                        .WithTitle("Thank you for adding me!")
                                        .WithColor(new DiscordColor("94f8ff"))
                                        .WithThumbnail(c.CurrentUser.AvatarUrl)
                                        .WithFooter("Silk!", c.CurrentUser.AvatarUrl)
                                        .WithTimestamp(DateTime.Now);

            var sb = new StringBuilder();
            sb.Append("Thank you for choosing Silk! to join your server <3")
              .AppendLine("I am a relatively lightweight bot with many functions - partially in moderation, ")
              .AppendLine("partially in games, with many more features to come!")
              .Append("If there's an issue, feel free to [Open an issue on GitHub](https://github.com/VelvetThePanda/Silkbot/issues), ")
              .AppendLine("or if you're not familiar with GitHub, feel free")
              .AppendLine($"to message the developers directly via {SilkBot.Bot.DefaultCommandPrefix}`ticket create <your message>`.")
              .Append($"By default, the prefix is `{SilkBot.Bot.DefaultCommandPrefix}`, or <@{c.CurrentUser.Id}>, but this can be changed by !setprefix <your prefix here>.");

            embed.WithDescription(sb.ToString());

            await firstChannel.SendMessageAsync(embed: embed);
        }

        // Set when !clear x is called, as to prevent logging messages cleared by the bot. //
        public static int UnloggedMessages { get; set; } 


        public static async Task OnMessageDeleted(DiscordClient c, MessageDeleteEventArgs e)
        {
            if (e.Message.Author is null) return; // Message isn't cached. //
            if (e.Guild is null) return; // Message is in private channel. //
            if (UnloggedMessages - 1 > 0)
            {
                UnloggedMessages--;
                e.Handled = true;
                return;
            }

            if (e.Channel.IsPrivate || e.Message.Author.IsCurrent) return;


            GuildModel config = c.GetCommandsNext().Services.Get<IDbContextFactory<SilkDbContext>>().CreateDbContext()
                                 .Guilds.First(g => g.Id == e.Guild.Id);

            if (!config.LogMessageChanges || config.MessageEditChannel == default) return;


            DiscordEmbedBuilder embed =
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
                    .WithFooter("Silk!", c.CurrentUser.AvatarUrl)
                    .WithTimestamp(DateTime.Now);
            DiscordChannel loggingChannel = await c.GetChannelAsync(config.MessageEditChannel);
            await c.SendMessageAsync(loggingChannel, embed: embed);
        }
    }
}