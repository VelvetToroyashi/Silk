using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SilkBot.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SilkBot.Commands.Moderation.Utilities
{
    public sealed class MessageEditHandler
    {


        public async Task OnMessageEdit(DiscordClient c, MessageUpdateEventArgs e)
        {
            if (e.Channel.IsPrivate) return;
            var config = SilkBot.Bot.Instance.SilkDBContext.Guilds.FirstOrDefault(g => g.DiscordGuildId == e.Guild.Id);
            if (config is null) return;
            CheckForInvite(e, config);
            var logChannel = config.MessageEditChannel;
            if (e.Message!.Author.IsCurrent || e.Message.Author!.IsBot || !e.Message.IsEdited) return;

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
                .WithFooter("Silk!", c.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);
            var loggingChannel = await c.GetChannelAsync(logChannel);
            _ = c.SendMessageAsync(loggingChannel, embed: embed);
        }
        private void CheckForInvite(MessageUpdateEventArgs e, GuildModel config)
        {
            if (config.WhitelistInvites)
            {
                if (e.Message.Content.Contains("discord.gg") || e.Message.Content.Contains("discord.com/invite"))
                {
                    var invite = Regex.Match(e.Message.Content, @"(discord\.gg\/.+)") ?? Regex.Match(e.Message.Content.ToLower(), @"(discord\.com\/invite\/.+)");
                    if (!invite.Success)
                    {
                        return;
                    }

                    var inviteLink = string.Join("", e.Message.Content.Skip(invite.Index).TakeWhile(c => c != ' ')).Replace("discord.com/invite", "discord.gg/");
                    if (!config.WhiteListedLinks.Any(link => link.Link == inviteLink))
                    {
                        e.Message.DeleteAsync().GetAwaiter();
                    }
                }
            }
        }
    }
}
