#region

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database;
using Silk.Core.Database.Models;

#endregion

namespace Silk.Core.Tools.EventHelpers
{
    public class MessageRemovedHelper
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public MessageRemovedHelper(IDbContextFactory<SilkDbContext> dbFactory) => _dbFactory = dbFactory;
        
        public async Task OnRemoved(DiscordClient c, MessageDeleteEventArgs e)
        {
            if (e.Message.Author.IsCurrent) return; // Self-evident. //
            if (e.Message.Author is null) return;   // Message isn't cached. //
            if (e.Channel.IsPrivate) return;        // Goes without saying. //
            if (e.Guild is null) return;            // Message is in private channel. //
            
            GuildModel guild = _dbFactory.CreateDbContext().Guilds.First(g => g.Id == e.Guild.Id);

            if (!guild.LogMessageChanges) return;
            DiscordEmbed embed = GetEditEmbed(e, DateTime.Now);
            DiscordChannel channel = await c.GetChannelAsync(guild.GeneralLoggingChannel);
            await channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        private DiscordEmbedBuilder GetEditEmbed(MessageDeleteEventArgs e, DateTime now) => new DiscordEmbedBuilder()
            .WithTitle("Message Deleted:")
            .WithDescription(
            $"User: {e.Message.Author.Mention}\n" +
            $"Channel: {e.Channel.Mention}\n" +
            $"Time: {now:HH:mm}\n" +
            $"Message Contents: ```\n{e.Message.Content}```")
            .AddField("Message ID:", e.Message.Id.ToString(), true)
            .AddField("User ID:", e.Message.Author.Id.ToString(), true)
            .WithThumbnail(e.Message.Author.AvatarUrl)
            .WithColor(DiscordColor.Red);
    }
}