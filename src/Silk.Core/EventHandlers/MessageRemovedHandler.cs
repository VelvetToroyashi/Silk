using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MediatR;
using Silk.Core.Services.Interfaces;
using Silk.Data.MediatR;
using Silk.Data.Models;

namespace Silk.Core.EventHandlers
{
    public class MessageRemovedHandler
    {
        private readonly IMediator _mediator;
        public MessageRemovedHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task MessageRemoved(DiscordClient c, MessageDeleteEventArgs e)
        {
            if (e.Message?.Author is null || e.Message is null) return; // Message isn't cached. //
            if (e.Message.Author.IsCurrent) return; // Self-evident.                  //
            if (e.Channel.IsPrivate) return; // Goes without saying.                           //
            _ = Task.Run(async () =>
            {
                GuildConfig config = await _mediator.Send(new GuildConfigRequest.Get(e.Guild.Id));

                if (!config.LogMessageChanges) return;
                if (config.LoggingChannel is 0) return;

                DiscordEmbed embed = GetEditEmbed(e, DateTime.Now);
                DiscordChannel channel = await c.GetChannelAsync(config.LoggingChannel);
                await channel.SendMessageAsync(embed).ConfigureAwait(false);
            });

        }

        private DiscordEmbedBuilder GetEditEmbed(MessageDeleteEventArgs e, DateTime now) => new DiscordEmbedBuilder()
            .WithTitle("Message Deleted:")
            .WithDescription(
                $"User: {e.Message.Author.Mention}\n" +
                $"Channel: {e.Channel.Mention}\n" +
                $"Message Contents: ```\n{e.Message.Content}```")
            .AddField("Message ID:", e.Message.Id.ToString(), true)
            .AddField("User ID:", e.Message.Author.Id.ToString(), true)
            .WithThumbnail(e.Message.Author.AvatarUrl)
            .WithFooter("Message deleted at (UTC)")
            .WithTimestamp(now.ToUniversalTime())
            .WithColor(DiscordColor.Red);
    }
}