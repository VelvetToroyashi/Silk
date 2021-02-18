using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Silk.Data;
using Silk.Data.Models;

namespace Silk.Core.Commands.Moderation.Utilities
{
    public sealed class MessageEditHandler
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;


        public MessageEditHandler(IDbContextFactory<SilkDbContext> dbFactory, DiscordShardedClient client)
        {
            _dbFactory = dbFactory;
            foreach (DiscordClient shard in client.ShardClients.Values) shard.MessageUpdated += OnMessageEdit;
        }

        public async Task OnMessageEdit(DiscordClient c, MessageUpdateEventArgs e)
        {
            if (e.Channel.IsPrivate) return;
            _ = Task.Run(async () =>
            {
                Guild config = _dbFactory.CreateDbContext().Guilds.First(g => g.Id == e.Guild.Id);
                CheckForInvite(e, config);
                ulong logChannel = config.Configuration.LoggingChannel;
                if (e.Message!.Author.IsCurrent || e.Message.Author!.IsBot || !e.Message.IsEdited) return;

                if (logChannel == default) return;

                DiscordEmbedBuilder embed =
                    new DiscordEmbedBuilder()
                        .WithAuthor($"{e.Message.Author.Username} ({e.Message.Author.Id})",
                            iconUrl: e.Message.Author.AvatarUrl)
                        .WithDescription($"[Message edited in]({e.Message.JumpLink}) {e.Message.Channel.Mention}:\n" +
                                         $"Time: {DateTime.Now:HH:mm}\n" +
                                         $"📝 **Original:**\n```\n{e.MessageBefore.Content}\n```\n" +
                                         $"📝 **Changed:**\n```\n{e.Message.Content}\n```\n")
                        .AddField("Message ID:", e.Message.Id.ToString(), true)
                        .AddField("Channel ID:", e.Channel.Id.ToString(), true)
                        .WithColor(DiscordColor.CornflowerBlue);
                DiscordChannel loggingChannel = await c.GetChannelAsync(logChannel);
                await c.SendMessageAsync(loggingChannel, embed);
            });
        }

        private void CheckForInvite(MessageUpdateEventArgs e, Guild config)
        {
            if (config.Configuration.BlacklistInvites)
                if (e.Message.Content.Contains("discord.gg") || e.Message.Content.Contains("discord.com/invite"))
                {
                    Match invite = Regex.Match(e.Message.Content, @"discord((app\.com|\.com)\/invite|\.gg\/.+)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
                    if (!invite.Success) return;

                    string inviteLink = string.Join("", e.Message.Content.Skip(invite.Index).TakeWhile(c => c != ' '))
                        .Replace("discord.com/invite", "discord.gg/");
                    if (config.Configuration.AllowedInvites.All(link => link.VanityURL != inviteLink))
                        e.Message.DeleteAsync().GetAwaiter();
                }
        }
    }
}